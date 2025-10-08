using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetBudgetsAsync(long userId, bool? isActive = null);
        Task<BudgetDto?> GetBudgetByIdAsync(long id, long userId);
        Task<BudgetDto> CreateBudgetAsync(long userId, CreateBudgetDto createDto);
        Task<BudgetDto?> UpdateBudgetAsync(long id, long userId, UpdateBudgetDto updateDto);
        Task<bool> DeleteBudgetAsync(long id, long userId);
        Task<BudgetSummaryDto> GetBudgetSummaryAsync(long userId);
        Task UpdateSpentAmountAsync(long budgetId);
        Task UpdateAllSpentAmountsAsync(long userId);
        Task<IEnumerable<BudgetDto>> GetOverBudgetCategoriesAsync(long userId);
        Task<decimal> GetBudgetUtilizationAsync(long userId, long? categoryId = null);
    }
}
