using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using techviet_be.Common.Options;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class PaymentService(
    IOptions<VnPayOptions> vnPayOptions,
    IGenericRepository<PaymentTransaction> paymentTransactionRepository,
    IOrderRepository orderRepository,
    IOrderStatusFlowService orderStatusFlowService) : IPaymentService
{
    private readonly VnPayOptions _vnPayOptions = vnPayOptions.Value;

    public async Task CreatePendingTransactionAsync(Guid orderId, string provider)
    {
        var parsedProvider = Enum.TryParse<PaymentProvider>(provider, true, out var providerValue)
            ? providerValue
            : PaymentProvider.VNPAY;

        var paymentTx = new PaymentTransaction
        {
            OrderId = orderId,
            Provider = parsedProvider,
            Status = PaymentTransactionStatus.PENDING
        };

        await paymentTransactionRepository.AddAsync(paymentTx);
        await paymentTransactionRepository.SaveChangesAsync();
    }

    public async Task<string> CreateVnPayPaymentUrlAsync(Guid orderId, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(_vnPayOptions.TmnCode) ||
            string.IsNullOrWhiteSpace(_vnPayOptions.HashSecret) ||
            string.IsNullOrWhiteSpace(_vnPayOptions.Url) ||
            string.IsNullOrWhiteSpace(_vnPayOptions.ReturnUrl))
        {
            throw new InvalidOperationException("VNPAY config is missing.");
        }

        var order = await orderRepository.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Total is null || order.Total <= 0)
        {
            throw new InvalidOperationException("Order total is invalid for payment.");
        }

        await CreatePendingTransactionAsync(orderId, PaymentProvider.VNPAY.ToString());

        var now = DateTime.UtcNow;
        var expire = now.AddMinutes(15);

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _vnPayOptions.TmnCode,
            ["vnp_Amount"] = ((long)(order.Total.Value * 100)).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {orderId}",
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = _vnPayOptions.ReturnUrl,
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ExpireDate"] = expire.ToString("yyyyMMddHHmmss")                            
        };

        var query = BuildQuery(vnpParams);
        var signData = BuildRawData(vnpParams);
        var secureHash = HmacSha512(_vnPayOptions.HashSecret, signData);

        return $"{_vnPayOptions.Url}?{query}&vnp_SecureHash={secureHash}";
    }

    public async Task HandleVnPayCallbackAsync(IDictionary<string, string> queryParams)
    {
        if (string.IsNullOrWhiteSpace(_vnPayOptions.HashSecret))
        {
            throw new InvalidOperationException("VNPAY hash secret is not configured.");
        }

        if (!queryParams.TryGetValue("vnp_SecureHash", out var secureHash) || string.IsNullOrWhiteSpace(secureHash))
        {
            throw new InvalidOperationException("Missing vnp_SecureHash.");
        }

        var isValidSignature = ValidateSignature(queryParams, _vnPayOptions.HashSecret, secureHash);
        if (!isValidSignature)
        {
            throw new InvalidOperationException("Invalid VNPAY checksum.");
        }

        if (!queryParams.TryGetValue("vnp_TxnRef", out var txnRef) || !Guid.TryParse(txnRef, out var orderId))
        {
            throw new InvalidOperationException("Invalid vnp_TxnRef.");
        }

        var order = await orderRepository.GetOrderWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.PaymentStatus == PaymentStatus.PAID)
        {
            return;
        }

        var responseCode = queryParams.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        var txNo = queryParams.TryGetValue("vnp_TransactionNo", out var txVal) ? txVal : null;

        var txRows = await paymentTransactionRepository.FindAsync(x => x.OrderId == orderId, asNoTracking: false);
        var latestTx = txRows.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        if (latestTx is null)
        {
            latestTx = new PaymentTransaction
            {
                OrderId = orderId,
                Provider = PaymentProvider.VNPAY,
                Status = PaymentTransactionStatus.PENDING
            };
            await paymentTransactionRepository.AddAsync(latestTx);
        }

        if (responseCode == "00")
        {
            order.PaymentStatus = PaymentStatus.PAID;
            orderStatusFlowService.EnsureValidTransition(order.OrderStatus, OrderStatus.PAID);
            order.OrderStatus = OrderStatus.PAID;

            latestTx.Status = PaymentTransactionStatus.SUCCESS;
            latestTx.TransactionId = txNo;
        }
        else
        {
            order.PaymentStatus = PaymentStatus.FAILED;
            latestTx.Status = PaymentTransactionStatus.FAILED;
            latestTx.TransactionId = txNo;
        }

        orderRepository.Update(order);
        paymentTransactionRepository.Update(latestTx);

        await orderRepository.SaveChangesAsync();
    }

    public async Task UpdatePaymentTransactionStatusAsync(Guid paymentTransactionId, string status, string? transactionId = null)
    {
        var paymentTransaction = await paymentTransactionRepository.GetByIdAsync(paymentTransactionId)
            ?? throw new KeyNotFoundException("Payment transaction not found.");

        var normalized = (status ?? string.Empty).Trim().ToUpperInvariant();
        var mappedStatus = normalized switch
        {
            "PAID" => PaymentTransactionStatus.SUCCESS,
            "SUCCESS" => PaymentTransactionStatus.SUCCESS,
            "FAILED" => PaymentTransactionStatus.FAILED,
            _ => throw new InvalidOperationException("Invalid payment transaction status. Use PAID or FAILED.")
        };

        var order = await orderRepository.GetByIdAsync(paymentTransaction.OrderId)
            ?? throw new KeyNotFoundException("Order not found.");

        paymentTransaction.Status = mappedStatus;
        paymentTransaction.TransactionId = transactionId ?? paymentTransaction.TransactionId;

        if (mappedStatus == PaymentTransactionStatus.SUCCESS)
        {
            order.PaymentStatus = PaymentStatus.PAID;
            if (order.OrderStatus == OrderStatus.PENDING)
            {
                orderStatusFlowService.EnsureValidTransition(order.OrderStatus, OrderStatus.PAID);
                order.OrderStatus = OrderStatus.PAID;
            }
        }
        else
        {
            order.PaymentStatus = PaymentStatus.FAILED;
        }

        paymentTransactionRepository.Update(paymentTransaction);
        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync();
    }

    private static bool ValidateSignature(IDictionary<string, string> queryParams, string secret, string secureHash)
    {
        var data = BuildHashData(queryParams);
        var calculatedHash = HmacSha512(secret, data);

        return string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRawData(IEnumerable<KeyValuePair<string, string>> queryParams)
    {
        var filtered = queryParams
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}");

        return string.Join("&", filtered);
    }

    private static string BuildQuery(IEnumerable<KeyValuePair<string, string>> queryParams)
    {
        return BuildRawData(queryParams);
    }

    private static string BuildHashData(IDictionary<string, string> queryParams)
    {
        var filtered = queryParams
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}");

        return string.Join("&", filtered);
    }

    private static string HmacSha512(string key, string inputData)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
        return Convert.ToHexString(hash);
    }
}