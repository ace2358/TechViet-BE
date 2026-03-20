namespace techviet_be.Dto;

public class CreateOrderDto
{
    public string? Fullname { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal ShippingFee { get; set; }
}
