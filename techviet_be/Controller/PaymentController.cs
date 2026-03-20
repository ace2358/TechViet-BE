using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[ApiController]
[Route("api/payment/vnpay")]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreatePaymentUrl([FromBody] CreateVnPayPaymentDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var paymentUrl = await paymentService.CreateVnPayPaymentUrlAsync(dto.OrderId, ip);

        return Ok(ApiResponse.Success(new PaymentUrlResponseDto
        {
            PaymentUrl = paymentUrl
        }));
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        await paymentService.HandleVnPayCallbackAsync(query);

        return Ok(ApiResponse.Success(new { }, "Callback processed"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("transactions/{paymentTransactionId:guid}/status")]
    public async Task<IActionResult> UpdatePaymentTransactionStatus(Guid paymentTransactionId, [FromBody] UpdatePaymentTransactionStatusDto dto)
    {
        await paymentService.UpdatePaymentTransactionStatusAsync(paymentTransactionId, dto.Status, dto.TransactionId);
        return Ok(ApiResponse.Success(new { paymentTransactionId }, "Payment transaction status updated"));
    }
}
