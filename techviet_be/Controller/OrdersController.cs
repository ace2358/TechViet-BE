using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = GetCurrentUserId();
        var orderId = await orderService.CreateOrderAsync(userId, dto);
        return Ok(ApiResponse.Success(new { orderId }, "Order created"));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var pagination = new PaginationParams
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : pageSize
        };

        var orders = await orderService.GetOrdersByUserAsync(userId, pagination);
        return Ok(ApiResponse.Success(orders));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetOrdersByUserId(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : pageSize
        };

        var orders = await orderService.GetOrdersByUserAsync(userId, pagination);
        return Ok(ApiResponse.Success(orders));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await orderService.UpdateOrderStatusAsync(orderId, dto.Status);
        return Ok(ApiResponse.Success(order, "Order status updated"));
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