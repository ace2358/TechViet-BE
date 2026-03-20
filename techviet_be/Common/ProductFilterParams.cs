namespace techviet_be.Common;

public class ProductFilterParams
{
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sort { get; set; }
}
