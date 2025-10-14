using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(long userId, int page = 1, int pageSize = 10,
        DateTime? startDate = null, DateTime? endDate = null, string? type = null, long? categoryId = null)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionDate <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                transactions = transactions.Where(t => t.Type == transactionType);
            }
        }

        if (categoryId.HasValue)
        {
            transactions = transactions.Where(t => t.CategoryId == categoryId.Value);
        }

        return transactions
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(long id, long userId)
    {
        return await _unitOfWork.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Update budget spent amount if applicable
        if (transaction.CategoryId.HasValue)
        {
            await UpdateBudgetSpentAmountAsync(transaction.CategoryId.Value, transaction.UserId);
        }

        _logger.LogInformation("Created transaction {TransactionId} for user {UserId}", transaction.Id, transaction.UserId);
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Update budget spent amount if applicable
        if (transaction.CategoryId.HasValue)
        {
            await UpdateBudgetSpentAmountAsync(transaction.CategoryId.Value, transaction.UserId);
        }

        _logger.LogInformation("Updated transaction {TransactionId} for user {UserId}", transaction.Id, transaction.UserId);
        return transaction;
    }

    public async Task<bool> DeleteTransactionAsync(long id, long userId)
    {
        var transaction = await GetTransactionByIdAsync(id, userId);
        if (transaction == null)
        {
            return false;
        }

        await _unitOfWork.Transactions.DeleteAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Update budget spent amount if applicable
        if (transaction.CategoryId.HasValue)
        {
            await UpdateBudgetSpentAmountAsync(transaction.CategoryId.Value, transaction.UserId);
        }

        _logger.LogInformation("Deleted transaction {TransactionId} for user {UserId}", id, userId);
        return true;
    }

    public async Task<(decimal totalIncome, decimal totalExpense, decimal netIncome)> GetUserSummaryAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t =>
            t.UserId == userId &&
            t.TransactionDate >= startDate &&
            t.TransactionDate <= endDate);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var netIncome = totalIncome - totalExpense;

        return (totalIncome, totalExpense, netIncome);
    }

    public async Task<IEnumerable<dynamic>> GetCategoryBreakdownAsync(long userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t =>
            t.UserId == userId &&
            t.Type == TransactionType.Expense &&
            t.TransactionDate >= startDate &&
            t.TransactionDate <= endDate);

        var categoryBreakdown = transactions
            .GroupBy(t => new { t.CategoryId, t.Category?.Name, t.Category?.Icon, t.Category?.Color })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name ?? "Uncategorized",
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return categoryBreakdown;
    }

    public async Task<IEnumerable<dynamic>> GetIncomeExpenseTrendAsync(long userId, int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var transactions = await _unitOfWork.Transactions.FindAsync(t =>
            t.UserId == userId &&
            t.TransactionDate >= startDate);

        var monthlyTrends = transactions
            .GroupBy(t => new
            {
                Year = t.TransactionDate.Year,
                Month = t.TransactionDate.Month
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                Net = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) - g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        return monthlyTrends;
    }

    private async Task UpdateBudgetSpentAmountAsync(long categoryId, long userId)
    {
        var budgets = await _unitOfWork.Budgets.FindAsync(b =>
            b.UserId == userId &&
            b.CategoryId == categoryId &&
            b.StartDate <= DateTime.UtcNow &&
            b.EndDate >= DateTime.UtcNow);

        foreach (var budget in budgets)
        {
            var spentAmount = await _unitOfWork.Transactions.FindAsync(t =>
                t.UserId == userId &&
                t.CategoryId == categoryId &&
                t.Type == TransactionType.Expense &&
                t.TransactionDate >= budget.StartDate &&
                t.TransactionDate <= budget.EndDate);

            // Note: SpentAmount is now calculated dynamically, not stored in the model
            // This method can be used to trigger budget notifications or other logic
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
