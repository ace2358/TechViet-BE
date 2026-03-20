using System.ComponentModel.DataAnnotations;

namespace techviet_be.Entity;

public class ProductSpec
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(100)]
    public string? SpecLabel { get; set; }

    public string? SpecValue { get; set; }

    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
