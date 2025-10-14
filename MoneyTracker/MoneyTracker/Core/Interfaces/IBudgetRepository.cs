using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<int> GetTotalCountAsync();
}
