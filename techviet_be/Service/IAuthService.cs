using techviet_be.Dto;

namespace techviet_be.Service;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> RegisterAdminAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<UserProfileDto> GetProfileAsync(Guid userId);
}
