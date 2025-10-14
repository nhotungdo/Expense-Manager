using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetSystemCategoriesAsync();
    Task<int> GetTotalCountAsync();
}
