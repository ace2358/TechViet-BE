using Microsoft.EntityFrameworkCore;
using techviet_be.Common;
using techviet_be.Entity;

namespace techviet_be.Repository;

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, bool asNoTracking = true)
    {
        var query = Context.Users.Where(x => x.Username == username);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email, bool asNoTracking = true)
    {
        var query = Context.Users.Where(x => x.Email == email);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<User> Data, int TotalCount)> GetUsersAsync(string? search, PaginationParams pagination)
    {
        var query = Context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(x =>
                x.Username.ToLower().Contains(keyword) ||
                x.Email.ToLower().Contains(keyword) ||
                x.Fullname.ToLower().Contains(keyword));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;

        var data = await query.Skip(skip).Take(pagination.PageSize).ToListAsync();
        return (data, totalCount);
    }
}
