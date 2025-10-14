using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(IUnitOfWork unitOfWork, ILogger<BudgetService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Budget>> GetUserBudgetsAsync(long userId)
    {
        return await _unitOfWork.Budgets.FindAsync(b => b.UserId == userId);
    }

    public async Task<Budget?> GetBudgetByIdAsync(long id, long userId)
    {
        return await _unitOfWork.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Budgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created budget {BudgetId} for user {UserId}", budget.Id, budget.UserId);
        return budget;
    }

    public async Task<Budget> UpdateBudgetAsync(Budget budget)
    {
        budget.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Budgets.UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated budget {BudgetId} for user {UserId}", budget.Id, budget.UserId);
        return budget;
    }

    public async Task<bool> DeleteBudgetAsync(long id, long userId)
    {
        var budget = await GetBudgetByIdAsync(id, userId);
        if (budget == null)
        {
            return false;
        }

        await _unitOfWork.Budgets.DeleteAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted budget {BudgetId} for user {UserId}", id, userId);
        return true;
    }

    public async Task UpdateBudgetSpentAmountAsync(long budgetId)
    {
        var budget = await _unitOfWork.Budgets.GetByIdAsync(budgetId);
        if (budget == null)
        {
            return;
        }

        var spentAmount = 0m;

        if (budget.CategoryId.HasValue)
        {
            // Calculate spent amount for specific category
            var transactions = await _unitOfWork.Transactions.FindAsync(t =>
                t.UserId == budget.UserId &&
                t.CategoryId == budget.CategoryId &&
                t.Type == TransactionType.Expense &&
                t.TransactionDate >= budget.StartDate &&
                t.TransactionDate <= budget.EndDate);

            spentAmount = transactions.Sum(t => t.Amount);
        }
        else
        {
            // Calculate total spent amount for all categories
            var transactions = await _unitOfWork.Transactions.FindAsync(t =>
                t.UserId == budget.UserId &&
                t.Type == TransactionType.Expense &&
                t.TransactionDate >= budget.StartDate &&
                t.TransactionDate <= budget.EndDate);

            spentAmount = transactions.Sum(t => t.Amount);
        }

        // Note: SpentAmount is now calculated dynamically, not stored in the model
        // This method can be used to trigger budget notifications or other logic
    }

    public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync(long userId)
    {
        var currentDate = DateTime.UtcNow;
        return await _unitOfWork.Budgets.FindAsync(b =>
            b.UserId == userId &&
            b.StartDate <= currentDate &&
            b.EndDate >= currentDate);
    }
}
