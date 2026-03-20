using techviet_be.Common;
using techviet_be.Dto;

namespace techviet_be.Service;

public interface IUserService
{
    Task<PagedResult<UserProfileDto>> GetUsersAsync(PaginationParams pagination, string? search = null);
    Task<UserProfileDto> GetByIdAsync(Guid userId);
    Task<UserProfileDto> UpdateAsync(Guid userId, UpdateUserManagementDto dto);
    Task DeleteAsync(Guid userId);
}
