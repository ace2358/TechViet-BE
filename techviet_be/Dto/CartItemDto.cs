namespace techviet_be.Dto;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal? Price { get; set; }
    public int Quantity { get; set; }
    public string? VariantInfo { get; set; }
}

public class AddCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? VariantInfo { get; set; }
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
    public string? VariantInfo { get; set; }
}

public class CartResponseDto
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
}
