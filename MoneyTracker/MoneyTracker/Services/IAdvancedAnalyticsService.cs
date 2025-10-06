using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IAdvancedAnalyticsService
    {
        Task<SpendingAnalysisDto> GetSpendingAnalysisAsync(long userId, DateTime startDate, DateTime endDate);
        Task<IncomeAnalysisDto> GetIncomeAnalysisAsync(long userId, DateTime startDate, DateTime endDate);
        Task<BudgetAnalysisDto> GetBudgetAnalysisAsync(long userId, DateTime startDate, DateTime endDate);
        Task<FinancialHealthDto> GetFinancialHealthAsync(long userId);
        Task<TrendAnalysisDto> GetTrendAnalysisAsync(long userId, int months = 12);
        Task<CategoryInsightsDto> GetCategoryInsightsAsync(long userId, DateTime startDate, DateTime endDate);
        Task<ForecastDto> GetFinancialForecastAsync(long userId, int months = 6);
    }
}
