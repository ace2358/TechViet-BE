using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    public class UploadProductImageRequest
    {
        public IFormFile File { get; set; } = null!;
        public int? SortOrder { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? brand,
        [FromQuery] string? search,
        [FromQuery(Name = "min_price")] decimal? minPrice,
        [FromQuery(Name = "max_price")] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ProductFilterParams
        {
            Category = category,
            Brand = brand,
            Search = search,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Sort = sort
        };

        var pagination = new PaginationParams
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : pageSize
        };

        var result = await productService.GetProductsAsync(filter, pagination);
        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound(ApiResponse.Error("Product not found", 404));
        }

        return Ok(ApiResponse.Success(product));
    }

    [HttpGet("{id:guid}/specs")]
    public async Task<IActionResult> GetProductSpecs(Guid id)
    {
        var specs = await productService.GetSpecsAsync(id);
        return Ok(ApiResponse.Success(specs));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] UpsertProductDto dto)
    {
        var created = await productService.CreateAsync(dto);
        return Ok(ApiResponse.Success(created, "Product created"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpsertProductDto dto)
    {
        var updated = await productService.UpdateAsync(id, dto);
        return Ok(ApiResponse.Success(updated, "Product updated"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await productService.DeleteAsync(id);
        return Ok(ApiResponse.Success(new { id }, "Product deleted"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{id:guid}/specs")]
    public async Task<IActionResult> CreateProductSpec(Guid id, [FromBody] CreateProductSpecDto dto)
    {
        var spec = await productService.CreateSpecAsync(id, dto);
        return Ok(ApiResponse.Success(spec, "Product spec created"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:guid}/specs/{specId:guid}")]
    public async Task<IActionResult> UpdateProductSpec(Guid id, Guid specId, [FromBody] UpdateProductSpecDto dto)
    {
        var spec = await productService.UpdateSpecAsync(id, specId, dto);
        return Ok(ApiResponse.Success(spec, "Product spec updated"));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:guid}/specs/{specId:guid}")]
    public async Task<IActionResult> DeleteProductSpec(Guid id, Guid specId)
    {
        await productService.DeleteSpecAsync(id, specId);
        return Ok(ApiResponse.Success(new { productId = id, specId }, "Product spec deleted"));
    }

    [Authorize(Roles = "ADMIN")]
    [Consumes("multipart/form-data")]
    [HttpPost("{id:guid}/images/upload")]
    public async Task<IActionResult> UploadProductImage(Guid id, [FromForm] UploadProductImageRequest request, CancellationToken cancellationToken)
    {
        var updated = await productService.UploadImageAsync(id, request.File, request.SortOrder, cancellationToken);
        return Ok(ApiResponse.Success(updated, "Product image uploaded"));
    }
}
