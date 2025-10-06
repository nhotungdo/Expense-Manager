using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IAdvancedSearchService
    {
        Task<SearchResultDto> SearchTransactionsAsync(long userId, AdvancedSearchDto searchDto);
        Task<List<string>> GetSearchSuggestionsAsync(long userId, string query, string type = "all");
        Task<Dictionary<string, object>> GetSearchFiltersAsync(long userId);
    }
}
