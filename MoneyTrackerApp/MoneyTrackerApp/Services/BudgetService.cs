using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing budgets with spending tracking and alerts
/// Handles budget CRUD, spending calculation, and over-budget warnings
/// </summary>
public interface IBudgetService
{
    Task<BudgetResponseDto?> GetBudgetByIdAsync(long budgetId, long userId);
    Task<List<BudgetResponseDto>> GetUserBudgetsAsync(long userId);
    Task<BudgetSummaryDto> GetBudgetSummaryAsync(long userId);
    Task<BudgetResponseDto> CreateBudgetAsync(long userId, CreateBudgetDto dto);
    Task<BudgetResponseDto> UpdateBudgetAsync(long userId, UpdateBudgetDto dto);
    Task<bool> DeleteBudgetAsync(long budgetId, long userId);
    Task<List<BudgetAlertDto>> GetBudgetAlertsAsync(long userId);
    Task<decimal> GetSpentAmountAsync(long userId, long? categoryId, long? accountId, DateTime startDate, DateTime endDate);
}

public class BudgetService : IBudgetService
{
    private readonly ExpenseManagerContext _context;
    private const decimal NEAR_LIMIT_THRESHOLD = 0.8m; // 80%

    public BudgetService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific budget by ID
    /// </summary>
    public async Task<BudgetResponseDto?> GetBudgetByIdAsync(long budgetId, long userId)
    {
        var budget = await _context.Budgets
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.Id == budgetId && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return null;

        return await MapToResponseDtoAsync(budget, userId);
    }

    /// <summary>
    /// Get all budgets for a user
    /// </summary>
    public async Task<List<BudgetResponseDto>> GetUserBudgetsAsync(long userId)
    {
        var budgets = await _context.Budgets
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var result = new List<BudgetResponseDto>();
        foreach (var budget in budgets)
        {
            result.Add(await MapToResponseDtoAsync(budget, userId));
        }

        return result;
    }

    /// <summary>
    /// Get budget summary with totals and alerts
    /// </summary>
    public async Task<BudgetSummaryDto> GetBudgetSummaryAsync(long userId)
    {
        var budgets = await GetUserBudgetsAsync(userId);

        return new BudgetSummaryDto
        {
            TotalBudgets = budgets.Count,
            OverBudgetCount = budgets.Count(b => b.IsOverBudget),
            NearLimitCount = budgets.Count(b => b.IsNearLimit && !b.IsOverBudget),
            TotalBudgeted = budgets.Sum(b => b.Amount),
            TotalSpent = budgets.Sum(b => b.Spent),
            TotalRemaining = budgets.Sum(b => b.Remaining),
            Budgets = budgets
        };
    }

    /// <summary>
    /// Create a new budget
    /// </summary>
    public async Task<BudgetResponseDto> CreateBudgetAsync(long userId, CreateBudgetDto dto)
    {
        // Validate that either CategoryId or AccountId is provided
        if (!dto.CategoryId.HasValue && !dto.AccountId.HasValue)
            throw new InvalidOperationException("Budget must be associated with either a category or an account");

        // Verify category if provided
        if (dto.CategoryId.HasValue)
        {
            var category = await _context.Categories
                .Where(c => c.Id == dto.CategoryId.Value && (c.UserId == userId || c.IsDefault))
                .FirstOrDefaultAsync();

            if (category == null)
                throw new InvalidOperationException("Category not found");
        }

        // Verify account if provided
        if (dto.AccountId.HasValue)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == dto.AccountId.Value && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (account == null)
                throw new InvalidOperationException("Account not found");
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            Amount = dto.Amount,
            Period = dto.Period,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(budget).Reference(b => b.Category).LoadAsync();
        await _context.Entry(budget).Reference(b => b.Account).LoadAsync();

        return await MapToResponseDtoAsync(budget, userId);
    }

    /// <summary>
    /// Update an existing budget
    /// </summary>
    public async Task<BudgetResponseDto> UpdateBudgetAsync(long userId, UpdateBudgetDto dto)
    {
        var budget = await _context.Budgets
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.Id == dto.Id && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            throw new InvalidOperationException("Budget not found");

        // Update fields
        if (dto.Amount.HasValue)
            budget.Amount = dto.Amount.Value;

        if (dto.StartDate.HasValue)
            budget.StartDate = dto.StartDate.Value;

        if (dto.EndDate.HasValue)
            budget.EndDate = dto.EndDate.Value;

        budget.UpdatedAt = DateTime.UtcNow;

        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(budget, userId);
    }

    /// <summary>
    /// Delete a budget
    /// </summary>
    public async Task<bool> DeleteBudgetAsync(long budgetId, long userId)
    {
        var budget = await _context.Budgets
            .Where(b => b.Id == budgetId && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return false;

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get budget alerts (near limit or over budget)
    /// </summary>
    public async Task<List<BudgetAlertDto>> GetBudgetAlertsAsync(long userId)
    {
        var budgets = await GetUserBudgetsAsync(userId);
        var alerts = new List<BudgetAlertDto>();

        foreach (var budget in budgets.Where(b => b.IsNearLimit || b.IsOverBudget))
        {
            var budgetName = budget.CategoryId.HasValue
                ? $"{budget.CategoryName} Budget"
                : $"{budget.AccountName} Budget";

            var alertType = budget.IsOverBudget ? "Over Budget" : "Near Limit";
            var message = budget.IsOverBudget
                ? $"You have exceeded your budget by {budget.Spent - budget.Amount:N0} VND"
                : $"You have used {budget.PercentageUsed:N0}% of your budget";

            alerts.Add(new BudgetAlertDto
            {
                BudgetId = budget.Id,
                BudgetName = budgetName,
                Amount = budget.Amount,
                Spent = budget.Spent,
                PercentageUsed = budget.PercentageUsed,
                AlertType = alertType,
                Message = message
            });
        }

        return alerts;
    }

    /// <summary>
    /// Calculate spent amount for a budget period
    /// </summary>
    public async Task<decimal> GetSpentAmountAsync(long userId, long? categoryId, long? accountId, DateTime startDate, DateTime endDate)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId
                && t.TransactionType == 2 // Expense only
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
    }

    // Helper Methods

    private async Task<BudgetResponseDto> MapToResponseDtoAsync(Budget budget, long userId)
    {
        var spent = await GetSpentAmountAsync(userId, budget.CategoryId, budget.AccountId, budget.StartDate, budget.EndDate);
        var remaining = budget.Amount - spent;
        var percentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0;
        var isOverBudget = spent > budget.Amount;
        var isNearLimit = percentageUsed >= (NEAR_LIMIT_THRESHOLD * 100) && !isOverBudget;

        return new BudgetResponseDto
        {
            Id = budget.Id,
            UserId = budget.UserId,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name,
            CategoryIcon = budget.Category?.Icon,
            CategoryColor = budget.Category?.Color,
            AccountId = budget.AccountId,
            AccountName = budget.Account?.Name,
            Amount = budget.Amount,
            Period = budget.Period,
            PeriodDisplay = GetPeriodDisplay(budget.Period),
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            Spent = spent,
            Remaining = remaining,
            PercentageUsed = percentageUsed,
            IsOverBudget = isOverBudget,
            IsNearLimit = isNearLimit,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };
    }

    private string GetPeriodDisplay(int period)
    {
        return period switch
        {
            1 => "Daily",
            2 => "Weekly",
            3 => "Monthly",
            4 => "Yearly",
            _ => "Unknown"
        };
    }
}
