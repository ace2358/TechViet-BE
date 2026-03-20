using techviet_be.Common;
using techviet_be.Dto;

namespace techviet_be.Service;

public interface IProductService
{
    Task<PagedResult<ProductResponseDto>> GetProductsAsync(ProductFilterParams filter, PaginationParams pagination);
    Task<ProductResponseDto?> GetByIdAsync(Guid id);
    Task<ProductResponseDto> CreateAsync(UpsertProductDto dto);
    Task<ProductResponseDto> UpdateAsync(Guid id, UpsertProductDto dto);
    Task<ProductResponseDto> UploadImageAsync(Guid id, IFormFile file, int? sortOrder, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductSpecDto>> GetSpecsAsync(Guid productId);
    Task<ProductSpecDto> CreateSpecAsync(Guid productId, CreateProductSpecDto dto);
    Task<ProductSpecDto> UpdateSpecAsync(Guid productId, Guid specId, UpdateProductSpecDto dto);
    Task DeleteSpecAsync(Guid productId, Guid specId);
    Task DeleteAsync(Guid id);
}
