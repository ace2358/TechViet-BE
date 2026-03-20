using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController(IProductService productService) : ControllerBase
{
    public class UploadFileRequest
    {
        public Guid ProductId { get; set; }
        public IFormFile File { get; set; } = null!;
        public int? SortOrder { get; set; }
    }

    [Consumes("multipart/form-data")]
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            throw new InvalidOperationException("productId is required.");
        }

        var updated = await productService.UploadImageAsync(request.ProductId, request.File, request.SortOrder, cancellationToken);
        return Ok(ApiResponse.Success(updated, "Product image uploaded"));
    }
}