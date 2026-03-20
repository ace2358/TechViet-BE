using Microsoft.EntityFrameworkCore;
using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<PagedResult<UserProfileDto>> GetUsersAsync(PaginationParams pagination, string? search = null)
    {
        var (users, totalCount) = await userRepository.GetUsersAsync(search, pagination);

        return new PagedResult<UserProfileDto>
        {
            Data = users.Select(MapProfile),
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<UserProfileDto> GetByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapProfile(user);
    }

    public async Task<UserProfileDto> UpdateAsync(Guid userId, UpdateUserManagementDto dto)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.Fullname))
        {
            user.Fullname = dto.Fullname.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            user.Phone = dto.Phone.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            user.Address = dto.Address.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
            {
                throw new InvalidOperationException("Invalid role.");
            }

            user.Role = parsedRole;
        }

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return MapProfile(user);
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        userRepository.Delete(user);

        try
        {
            await userRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Cannot delete user with related data.");
        }
    }

    private static UserProfileDto MapProfile(User user)
    {
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
}
