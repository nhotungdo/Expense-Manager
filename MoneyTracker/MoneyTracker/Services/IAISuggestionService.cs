using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IAISuggestionService
    {
        Task<IEnumerable<AiSuggestion>> GetSuggestionsAsync(long userId, int skip = 0, int take = 10);
        Task<AiSuggestion> GenerateSuggestionAsync(long userId);
        Task<bool> MarkSuggestionAsReadAsync(long suggestionId, long userId);
        Task<Dictionary<string, object>> GetSpendingAnalysisAsync(long userId);
        Task<List<string>> GenerateBudgetRecommendationsAsync(long userId);
        Task<List<string>> GenerateSpendingInsightsAsync(long userId);
    }
}
