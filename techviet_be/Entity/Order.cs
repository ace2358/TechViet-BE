using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace techviet_be.Entity;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string? Fullname { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Note { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;

    public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING;

    [Column(TypeName = "numeric(12,2)")]
    public decimal? Subtotal { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal? ShippingFee { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal? Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
