using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using techviet_be.Common.Options;
using techviet_be.Dto;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        return await RegisterInternalAsync(dto, UserRole.USER);
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(RegisterRequestDto dto)
    {
        return await RegisterInternalAsync(dto, UserRole.ADMIN);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var loginValue = string.IsNullOrWhiteSpace(dto.UsernameOrEmail)
            ? dto.Username
            : dto.UsernameOrEmail;

        if (string.IsNullOrWhiteSpace(loginValue) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new InvalidOperationException("Username/email and password are required.");
        }

        var user = await userRepository.GetByUsernameAsync(loginValue)
            ?? await userRepository.GetByEmailAsync(loginValue)
            ?? throw new InvalidOperationException("Invalid username/email or password.");

        var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new InvalidOperationException("Invalid username/email or password.");
        }

        return new AuthResponseDto
        {
            Token = GenerateToken(user)
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Fullname = user.Fullname,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<AuthResponseDto> RegisterInternalAsync(RegisterRequestDto dto, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Fullname) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Phone) ||
            string.IsNullOrWhiteSpace(dto.Address) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new InvalidOperationException("Username, fullname, email, phone, address and password are required.");
        }

        var existedUserByName = await userRepository.GetByUsernameAsync(dto.Username);
        if (existedUserByName is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var existedUserByEmail = await userRepository.GetByEmailAsync(dto.Email);
        if (existedUserByEmail is not null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Username = dto.Username.Trim(),
            Fullname = dto.Fullname.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateToken(user)
        };
    }

    private string GenerateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT secret is missing.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
