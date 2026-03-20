namespace techviet_be.Entity;

public class PaymentTransaction
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public PaymentProvider Provider { get; set; } = PaymentProvider.VNPAY;

    public string? TransactionId { get; set; }

    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.PENDING;

    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
}