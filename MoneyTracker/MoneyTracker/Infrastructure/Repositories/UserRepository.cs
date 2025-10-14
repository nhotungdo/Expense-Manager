using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Core.Models;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.FirstName!.Contains(search) ||
                u.LastName!.Contains(search) ||
                u.Email!.Contains(search) ||
                u.FullName!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetRecentUsersAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Users
            .Where(u => u.CreatedAt >= cutoffDate)
            .CountAsync();
    }

    public async Task<int> GetMonthlyCountAsync(int year, int month)
    {
        return await _context.Users
            .Where(u => u.CreatedAt!.Value.Year == year && u.CreatedAt.Value.Month == month)
            .CountAsync();
    }
}
