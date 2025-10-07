using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IIncomeService
    {
        Task<IEnumerable<Income>> GetIncomesAsync(long userId, int skip = 0, int take = 50);
        Task<Income?> GetIncomeByIdAsync(long id, long userId);
        Task<Income> CreateIncomeAsync(IncomeDto incomeDto, long userId);
        Task<Income?> UpdateIncomeAsync(long id, IncomeDto incomeDto, long userId);
        Task<bool> DeleteIncomeAsync(long id, long userId);
        Task<decimal> GetTotalIncomeAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Income>> GetIncomesByCategoryAsync(long userId, long categoryId, int skip = 0, int take = 50);
        Task<Dictionary<string, decimal>> GetIncomeByCategorySummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Income>> GetRecentIncomesAsync(long userId, int count = 10);
    }
}
