namespace techviet_be.Dto;

public class CreateVnPayPaymentDto
{
    public Guid OrderId { get; set; }
}

public class PaymentUrlResponseDto
{
    public string PaymentUrl { get; set; } = string.Empty;
}

public class UpdatePaymentTransactionStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}
