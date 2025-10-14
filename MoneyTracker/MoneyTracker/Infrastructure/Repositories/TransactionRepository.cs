using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Transactions.CountAsync();
    }

    public async Task<int> GetRecentTransactionsAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Transactions
            .Where(t => t.CreatedAt >= cutoffDate)
            .CountAsync();
    }

    public async Task<int> GetMonthlyCountAsync(int year, int month)
    {
        return await _context.Transactions
            .Where(t => t.CreatedAt!.Value.Year == year && t.CreatedAt.Value.Month == month)
            .CountAsync();
    }

    public async Task<bool> IsCategoryUsedAsync(long categoryId)
    {
        return await _context.Transactions
            .AnyAsync(t => t.CategoryId == categoryId);
    }
}
