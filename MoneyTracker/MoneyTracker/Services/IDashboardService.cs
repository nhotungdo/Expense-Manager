using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(long userId);
        Task<object> GetMonthlyReportAsync(long userId, int? month = null, int? year = null);
        Task<object> GetBudgetAnalysisAsync(long userId);
        Task<object> GetSpendingTrendsAsync(long userId, int months = 6);
        Task<object> GetCategoryBreakdownAsync(long userId, string type = "expense");
        Task<object> GetRecentActivityAsync(long userId, int count = 10);
        Task<AiSuggestion> GenerateAiSuggestionAsync(long userId);
    }
}
