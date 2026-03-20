using techviet_be.Common;
using techviet_be.Entity;

namespace techviet_be.Repository;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, bool asNoTracking = true);
    Task<User?> GetByEmailAsync(string email, bool asNoTracking = true);
    Task<(IEnumerable<User> Data, int TotalCount)> GetUsersAsync(string? search, PaginationParams pagination);
}
