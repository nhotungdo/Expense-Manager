using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<int> GetTotalCountAsync();
    Task<int> GetRecentTransactionsAsync(int days);
    Task<int> GetMonthlyCountAsync(int year, int month);
    Task<bool> IsCategoryUsedAsync(long categoryId);
}
