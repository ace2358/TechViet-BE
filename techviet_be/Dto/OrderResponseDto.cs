using techviet_be.Entity;

namespace techviet_be.Dto;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal? Total { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = [];
}

public class OrderItemResponseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}