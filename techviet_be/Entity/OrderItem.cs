using System.ComponentModel.DataAnnotations.Schema;

namespace techviet_be.Entity;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal Price { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
