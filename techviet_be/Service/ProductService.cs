using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class ProductService(IProductRepository productRepository, IImageService imageService) : IProductService
{
    public async Task<PagedResult<ProductResponseDto>> GetProductsAsync(ProductFilterParams filter, PaginationParams pagination)
    {
        var (data, totalCount) = await productRepository.GetProductsAsync(filter, pagination);

        return new PagedResult<ProductResponseDto>
        {
            Data = data.Select(MapProduct),
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await productRepository.GetProductWithDetailsAsync(id);
        return product is null ? null : MapProduct(product);
    }

    public async Task<ProductResponseDto> CreateAsync(UpsertProductDto dto)
    {
        if (dto.Price < 0)
        {
            throw new InvalidOperationException("Price must be greater than or equal to 0.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Brand = dto.Brand,
            Category = dto.Category,
            Description = dto.Description,
            Price = dto.Price,
            OriginalPrice = dto.OriginalPrice,
            DiscountPercent = dto.DiscountPercent,
            StockQty = dto.StockQty,
            IsActive = dto.IsActive,
            Images = dto.Images.Select(x => new ProductImage
            {
                ImageUrl = x.ImageUrl,
                SortOrder = x.SortOrder
            }).ToList(),
            Specs = dto.Specs.Select(x => new ProductSpec
            {
                SpecLabel = x.SpecLabel,
                SpecValue = x.SpecValue,
                SortOrder = x.SortOrder
            }).ToList()
        };

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();

        var created = await productRepository.GetProductWithDetailsAsync(product.Id)
            ?? throw new InvalidOperationException("Cannot load created product.");

        return MapProduct(created);
    }

    public async Task<ProductResponseDto> UpdateAsync(Guid id, UpsertProductDto dto)
    {
        var product = await productRepository.GetProductForUpdateAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        product.Name = dto.Name;
        product.Brand = dto.Brand;
        product.Category = dto.Category;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.OriginalPrice = dto.OriginalPrice;
        product.DiscountPercent = dto.DiscountPercent;
        product.StockQty = dto.StockQty;
        product.IsActive = dto.IsActive;

        product.Images.Clear();
        foreach (var image in dto.Images)
        {
            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = image.ImageUrl,
                SortOrder = image.SortOrder
            });
        }

        product.Specs.Clear();
        foreach (var spec in dto.Specs)
        {
            product.Specs.Add(new ProductSpec
            {
                ProductId = product.Id,
                SpecLabel = spec.SpecLabel,
                SpecValue = spec.SpecValue,
                SortOrder = spec.SortOrder
            });
        }

        productRepository.Update(product);
        await productRepository.SaveChangesAsync();

        var updated = await productRepository.GetProductWithDetailsAsync(product.Id)
            ?? throw new InvalidOperationException("Cannot load updated product.");

        return MapProduct(updated);
    }

    public async Task<ProductResponseDto> UploadImageAsync(Guid id, IFormFile file, int? sortOrder, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetProductForUpdateAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        var imageUrl = await imageService.UploadAsync(file, cancellationToken);

        var nextSortOrder = sortOrder ??
            (product.Images.Count == 0 ? 0 : product.Images.Max(x => x.SortOrder) + 1);

        product.Images.Add(new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = imageUrl,
            SortOrder = nextSortOrder
        });

        productRepository.Update(product);
        await productRepository.SaveChangesAsync();

        var updated = await productRepository.GetProductWithDetailsAsync(product.Id)
            ?? throw new InvalidOperationException("Cannot load updated product.");

        return MapProduct(updated);
    }

    public async Task<IEnumerable<ProductSpecDto>> GetSpecsAsync(Guid productId)
    {
        var product = await productRepository.GetProductWithDetailsAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        return product.Specs.OrderBy(x => x.SortOrder).Select(MapSpec).ToList();
    }

    public async Task<ProductSpecDto> CreateSpecAsync(Guid productId, CreateProductSpecDto dto)
    {
        var product = await productRepository.GetProductForUpdateAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        var spec = new ProductSpec
        {
            ProductId = product.Id,
            SpecLabel = dto.SpecLabel,
            SpecValue = dto.SpecValue,
            SortOrder = dto.SortOrder
        };

        product.Specs.Add(spec);
        productRepository.Update(product);
        await productRepository.SaveChangesAsync();

        return MapSpec(spec);
    }

    public async Task<ProductSpecDto> UpdateSpecAsync(Guid productId, Guid specId, UpdateProductSpecDto dto)
    {
        var product = await productRepository.GetProductForUpdateAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        var spec = product.Specs.FirstOrDefault(x => x.Id == specId)
            ?? throw new KeyNotFoundException("Spec not found.");

        spec.SpecLabel = dto.SpecLabel;
        spec.SpecValue = dto.SpecValue;
        spec.SortOrder = dto.SortOrder;

        productRepository.Update(product);
        await productRepository.SaveChangesAsync();

        return MapSpec(spec);
    }

    public async Task DeleteSpecAsync(Guid productId, Guid specId)
    {
        var product = await productRepository.GetProductForUpdateAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        var spec = product.Specs.FirstOrDefault(x => x.Id == specId)
            ?? throw new KeyNotFoundException("Spec not found.");

        product.Specs.Remove(spec);
        productRepository.Update(product);
        await productRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        product.IsActive = false;
        productRepository.Update(product);
        await productRepository.SaveChangesAsync();
    }

    private static ProductResponseDto MapProduct(Entity.Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            Category = product.Category,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            DiscountPercent = product.DiscountPercent,
            StockQty = product.StockQty,
            RatingAvg = product.RatingAvg,
            ReviewCount = product.ReviewCount,
            Thumbnail = product.Images.OrderBy(x => x.SortOrder).Select(x => x.ImageUrl).FirstOrDefault(),
            Images = product.Images.OrderBy(x => x.SortOrder).Select(x => new ProductImageDto
            {
                ImageUrl = x.ImageUrl,
                SortOrder = x.SortOrder
            }).ToList(),
            Specs = product.Specs.OrderBy(x => x.SortOrder).Select(x => new ProductSpecDto
            {
                Id = x.Id,
                SpecLabel = x.SpecLabel,
                SpecValue = x.SpecValue,
                SortOrder = x.SortOrder
            }).ToList()
        };
    }

    private static ProductSpecDto MapSpec(ProductSpec spec)
    {
        return new ProductSpecDto
        {
            Id = spec.Id,
            SpecLabel = spec.SpecLabel,
            SpecValue = spec.SpecValue,
            SortOrder = spec.SortOrder
        };
    }
}
