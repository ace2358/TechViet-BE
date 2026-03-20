using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace techviet_be.Entity;

public class Product
{
    public Guid Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal? OriginalPrice { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal DiscountPercent { get; set; }

    public int StockQty { get; set; }

    [Column(TypeName = "numeric(3,2)")]
    public decimal RatingAvg { get; set; }

    public int ReviewCount { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public ICollection<ProductSpec> Specs { get; set; } = new List<ProductSpec>();

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
