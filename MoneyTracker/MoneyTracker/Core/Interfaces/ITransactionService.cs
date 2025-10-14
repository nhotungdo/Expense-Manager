using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetUserTransactionsAsync(long userId, int page = 1, int pageSize = 10, 
        DateTime? startDate = null, DateTime? endDate = null, string? type = null, long? categoryId = null);
    Task<Transaction?> GetTransactionByIdAsync(long id, long userId);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task<bool> DeleteTransactionAsync(long id, long userId);
    Task<(decimal totalIncome, decimal totalExpense, decimal netIncome)> GetUserSummaryAsync(long userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<dynamic>> GetCategoryBreakdownAsync(long userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<dynamic>> GetIncomeExpenseTrendAsync(long userId, int months = 12);
}
