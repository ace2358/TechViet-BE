using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using techviet_be.Common;
using techviet_be.Entity;

namespace techviet_be.Repository;

public class OrderRepository(AppDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(Guid userId)
    {
        return await Context.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Order> Data, int TotalCount)> GetOrdersByUserAsync(Guid userId, PaginationParams pagination)
    {
        var query = Context.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;

        var data = await query
            .Skip(skip)
            .Take(pagination.PageSize)
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
    {
        return await Context.Orders
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == orderId);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return Context.Database.BeginTransactionAsync();
    }
}
