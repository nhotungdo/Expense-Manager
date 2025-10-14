using MoneyTracker.Models;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<int> GetTotalCountAsync();
    Task<int> GetRecentUsersAsync(int days);
    Task<int> GetMonthlyCountAsync(int year, int month);
}
