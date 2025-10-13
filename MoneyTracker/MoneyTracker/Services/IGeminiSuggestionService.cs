using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public interface IGeminiSuggestionService
    {
        Task<string> GetFinancialSuggestionAsync(IEnumerable<Transaction> recentTransactions);
    }
}
