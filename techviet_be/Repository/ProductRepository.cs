using Microsoft.EntityFrameworkCore;
using techviet_be.Common;
using techviet_be.Entity;

namespace techviet_be.Repository;

public class ProductRepository(AppDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsAsync(ProductFilterParams filter, PaginationParams pagination)
    {
        var query = Context.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            query = query.Where(x => x.Brand == filter.Brand);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var keyword = filter.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(keyword));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);
        }

        query = filter.Sort?.ToLower() switch
        {
            "price_asc" => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            _ => query.OrderByDescending(x => x.Id)
        };

        var totalCount = await query.CountAsync();
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var data = await query
            .Skip(skip)
            .Take(pagination.PageSize)
            .Include(x => x.Images)
            .Include(x => x.Specs)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<Product?> GetProductWithDetailsAsync(Guid id)
    {
        return await Context.Products
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .Include(x => x.Specs.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<Product?> GetProductForUpdateAsync(Guid id)
    {
        return await Context.Products
            .Include(x => x.Images)
            .Include(x => x.Specs)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
