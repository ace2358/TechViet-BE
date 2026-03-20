using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetCurrentUserId();
        var cart = await cartService.GetCartAsync(userId);
        return Ok(ApiResponse.Success(cart));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetCartByUserId(Guid userId)
    {
        var cart = await cartService.GetCartAsync(userId);
        return Ok(ApiResponse.Success(cart));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
    {
        var userId = GetCurrentUserId();
        var cart = await cartService.AddItemAsync(userId, dto);
        return Ok(ApiResponse.Success(cart, "Item added"));
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateCartItemDto dto)
    {
        var userId = GetCurrentUserId();
        var cart = await cartService.UpdateItemAsync(userId, id, dto);
        return Ok(ApiResponse.Success(cart, "Item updated"));
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var userId = GetCurrentUserId();
        await cartService.RemoveItemAsync(userId, id);
        return Ok(ApiResponse.Success(new { id }, "Item deleted"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            throw new InvalidOperationException("Invalid user token.");
        }

        return userId;
    }
}
