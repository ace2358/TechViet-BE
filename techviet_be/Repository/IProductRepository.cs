using techviet_be.Common;
using techviet_be.Entity;

namespace techviet_be.Repository;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(IEnumerable<Product> Data, int TotalCount)> GetProductsAsync(ProductFilterParams filter, PaginationParams pagination);
    Task<Product?> GetProductWithDetailsAsync(Guid id);
    Task<Product?> GetProductForUpdateAsync(Guid id);
}
