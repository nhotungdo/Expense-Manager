using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsAsync(long userId, TransactionFilterDto filter);
        Task<TransactionDto?> GetTransactionByIdAsync(long id, long userId);
        Task<TransactionDto> CreateTransactionAsync(long userId, CreateTransactionDto createDto);
        Task<TransactionDto?> UpdateTransactionAsync(long id, long userId, UpdateTransactionDto updateDto);
        Task<bool> DeleteTransactionAsync(long id, long userId);
        Task<TransactionSummaryDto> GetTransactionSummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TransactionDto>> GetRecentTransactionsAsync(long userId, int count = 10);
        Task<decimal> GetTotalIncomeAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpenseAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
