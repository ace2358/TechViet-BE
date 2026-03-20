using techviet_be.Common;
using techviet_be.Dto;

namespace techviet_be.Service;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByUserAsync(Guid userId);
    Task<PagedResult<OrderResponseDto>> GetOrdersByUserAsync(Guid userId, PaginationParams pagination);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, string status);
}
