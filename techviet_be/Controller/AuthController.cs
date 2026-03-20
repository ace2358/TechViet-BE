using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Service;

namespace techviet_be.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return Ok(ApiResponse.Success(result, "Register success"));
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequestDto dto)
    {
        var result = await authService.RegisterAdminAsync(dto);
        return Ok(ApiResponse.Success(result, "Admin register success"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(ApiResponse.Success(result, "Login success"));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            throw new InvalidOperationException("Invalid user token.");
        }

        var profile = await authService.GetProfileAsync(userId);
        return Ok(ApiResponse.Success(profile));
    }
}
