namespace techviet_be.Dto;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public int StockQty { get; set; }
    public decimal RatingAvg { get; set; }
    public int ReviewCount { get; set; }
    public string? Thumbnail { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductSpecDto> Specs { get; set; } = [];
}

public class ProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ProductSpecDto
{
    public Guid Id { get; set; }
    public string? SpecLabel { get; set; }
    public string? SpecValue { get; set; }
    public int SortOrder { get; set; }
}

public class CreateProductSpecDto
{
    public string? SpecLabel { get; set; }
    public string? SpecValue { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateProductSpecDto
{
    public string? SpecLabel { get; set; }
    public string? SpecValue { get; set; }
    public int SortOrder { get; set; }
}

public class UpsertProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public int StockQty { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductSpecDto> Specs { get; set; } = [];
}
