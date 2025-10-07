using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<Expense>> GetExpensesAsync(long userId, int skip = 0, int take = 50);
        Task<Expense?> GetExpenseByIdAsync(long id, long userId);
        Task<Expense> CreateExpenseAsync(ExpenseDto expenseDto, long userId);
        Task<Expense?> UpdateExpenseAsync(long id, ExpenseDto expenseDto, long userId);
        Task<bool> DeleteExpenseAsync(long id, long userId);
        Task<decimal> GetTotalExpensesAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(long userId, long categoryId, int skip = 0, int take = 50);
        Task<Dictionary<string, decimal>> GetExpensesByCategorySummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Expense>> GetRecentExpensesAsync(long userId, int count = 10);
    }
}
