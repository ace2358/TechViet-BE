namespace techviet_be.Service;

public interface IPaymentService
{
    Task CreatePendingTransactionAsync(Guid orderId, string provider);
    Task<string> CreateVnPayPaymentUrlAsync(Guid orderId, string ipAddress);
    Task HandleVnPayCallbackAsync(IDictionary<string, string> queryParams);
    Task UpdatePaymentTransactionStatusAsync(Guid paymentTransactionId, string status, string? transactionId = null);
}
