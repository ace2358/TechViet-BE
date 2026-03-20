using techviet_be.Common;
using Microsoft.EntityFrameworkCore.Storage;
using techviet_be.Entity;

namespace techviet_be.Repository;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserAsync(Guid userId);
    Task<(IEnumerable<Order> Data, int TotalCount)> GetOrdersByUserAsync(Guid userId, PaginationParams pagination);
    Task<Order?> GetOrderWithItemsAsync(Guid orderId);
    Task<IDbContextTransaction> BeginTransactionAsync();
}
