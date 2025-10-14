using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetUserBudgetsAsync(long userId);
    Task<Budget?> GetBudgetByIdAsync(long id, long userId);
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<Budget> UpdateBudgetAsync(Budget budget);
    Task<bool> DeleteBudgetAsync(long id, long userId);
    Task UpdateBudgetSpentAmountAsync(long budgetId);
    Task<IEnumerable<Budget>> GetActiveBudgetsAsync(long userId);
}
