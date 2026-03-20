using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var pagination = new PaginationParams
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : pageSize
        };

        var users = await userService.GetUsersAsync(pagination, search);
        return Ok(ApiResponse.Success(users));
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await userService.GetByIdAsync(userId);
        return Ok(ApiResponse.Success(user));
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserManagementDto dto)
    {
        var user = await userService.UpdateAsync(userId, dto);
        return Ok(ApiResponse.Success(user, "User updated"));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await userService.DeleteAsync(userId);
        return Ok(ApiResponse.Success(new { userId }, "User deleted"));
    }
}
