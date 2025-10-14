using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IAiService
{
    Task<IEnumerable<AiSuggestion>> GetSuggestionsAsync(long userId);
    Task<AiSuggestion> GenerateSuggestionAsync(long userId, string suggestionType);
    Task<IEnumerable<AiSuggestion>> GetBudgetSuggestionsAsync(long userId);
    Task<IEnumerable<AiSuggestion>> GetSpendingSuggestionsAsync(long userId);
    Task<IEnumerable<AiSuggestion>> GetSavingsSuggestionsAsync(long userId);
}
