using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(ExpenseManagerContext context, ILogger<BudgetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync(long userId, bool? isActive = null)
        {
            try
            {
                var query = _context.Budgets
                    .Where(b => b.UserId == userId)
                    .Include(b => b.Category)
                    .AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(b => b.IsActive == isActive.Value);
                }

                var budgets = await query
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        CategoryId = b.CategoryId,
                        CategoryName = b.Category != null ? b.Category.Name : "Tổng quát",
                        BudgetAmount = b.BudgetAmount,
                        SpentAmount = b.SpentAmount,
                        RemainingAmount = b.BudgetAmount - b.SpentAmount,
                        PercentageUsed = b.BudgetAmount > 0 ? (b.SpentAmount / b.BudgetAmount) * 100 : 0,
                        Currency = b.Currency,
                        PeriodType = b.PeriodType,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return budgets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budgets for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BudgetDto?> GetBudgetByIdAsync(long id, long userId)
        {
            try
            {
                var budget = await _context.Budgets
                    .Where(b => b.Id == id && b.UserId == userId)
                    .Include(b => b.Category)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        CategoryId = b.CategoryId,
                        CategoryName = b.Category != null ? b.Category.Name : "Tổng quát",
                        BudgetAmount = b.BudgetAmount,
                        SpentAmount = b.SpentAmount,
                        RemainingAmount = b.BudgetAmount - b.SpentAmount,
                        PercentageUsed = b.BudgetAmount > 0 ? (b.SpentAmount / b.BudgetAmount) * 100 : 0,
                        Currency = b.Currency,
                        PeriodType = b.PeriodType,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return budget;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget {BudgetId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<BudgetDto> CreateBudgetAsync(long userId, CreateBudgetDto createDto)
        {
            try
            {
                // Check if budget already exists for the same category and period
                var existingBudget = await _context.Budgets
                    .Where(b => b.UserId == userId &&
                               b.CategoryId == createDto.CategoryId &&
                               b.StartDate <= createDto.EndDate &&
                               b.EndDate >= createDto.StartDate &&
                               b.IsActive)
                    .FirstOrDefaultAsync();

                if (existingBudget != null)
                {
                    throw new InvalidOperationException("Budget already exists for this category and period");
                }

                var budget = new Budget
                {
                    UserId = userId,
                    CategoryId = createDto.CategoryId,
                    BudgetAmount = createDto.BudgetAmount,
                    SpentAmount = 0,
                    Currency = createDto.Currency ?? "VND",
                    PeriodType = createDto.PeriodType,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();

                // Update spent amount
                await UpdateSpentAmountAsync(budget.Id);

                // Return the created budget
                return await GetBudgetByIdAsync(budget.Id, userId) ??
                    throw new InvalidOperationException("Failed to retrieve created budget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BudgetDto?> UpdateBudgetAsync(long id, long userId, UpdateBudgetDto updateDto)
        {
            try
            {
                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (budget == null)
                {
                    return null;
                }

                budget.CategoryId = updateDto.CategoryId;
                budget.BudgetAmount = updateDto.BudgetAmount;
                budget.Currency = updateDto.Currency ?? "VND";
                budget.PeriodType = updateDto.PeriodType;
                budget.StartDate = updateDto.StartDate;
                budget.EndDate = updateDto.EndDate;
                budget.IsActive = updateDto.IsActive;
                budget.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update spent amount
                await UpdateSpentAmountAsync(budget.Id);

                return await GetBudgetByIdAsync(id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget {BudgetId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeleteBudgetAsync(long id, long userId)
        {
            try
            {
                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (budget == null)
                {
                    return false;
                }

                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget {BudgetId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<BudgetSummaryDto> GetBudgetSummaryAsync(long userId)
        {
            try
            {
                var activeBudgets = await _context.Budgets
                    .Where(b => b.UserId == userId && b.IsActive)
                    .Include(b => b.Category)
                    .ToListAsync();

                var totalBudget = activeBudgets.Sum(b => b.BudgetAmount);
                var totalSpent = activeBudgets.Sum(b => b.SpentAmount);
                var totalRemaining = totalBudget - totalSpent;
                var percentageUsed = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;
                var overBudgetCategories = activeBudgets.Count(b => b.SpentAmount > b.BudgetAmount);

                var budgetDtos = activeBudgets.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : "Tổng quát",
                    BudgetAmount = b.BudgetAmount,
                    SpentAmount = b.SpentAmount,
                    RemainingAmount = b.BudgetAmount - b.SpentAmount,
                    PercentageUsed = b.BudgetAmount > 0 ? (b.SpentAmount / b.BudgetAmount) * 100 : 0,
                    Currency = b.Currency,
                    PeriodType = b.PeriodType,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                }).ToList();

                return new BudgetSummaryDto
                {
                    TotalBudget = totalBudget,
                    TotalSpent = totalSpent,
                    TotalRemaining = totalRemaining,
                    PercentageUsed = percentageUsed,
                    ActiveBudgets = activeBudgets.Count,
                    OverBudgetCategories = overBudgetCategories,
                    Budgets = budgetDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget summary for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateSpentAmountAsync(long budgetId)
        {
            try
            {
                var budget = await _context.Budgets.FindAsync(budgetId);
                if (budget == null) return;

                var spentAmount = 0m;

                if (budget.CategoryId.HasValue)
                {
                    // Calculate spent amount for specific category
                    spentAmount = await _context.Expenses
                        .Where(e => e.UserId == budget.UserId &&
                                   e.CategoryId == budget.CategoryId &&
                                   e.ExpenseDate >= budget.StartDate &&
                                   e.ExpenseDate <= budget.EndDate)
                        .SumAsync(e => e.Amount);
                }
                else
                {
                    // Calculate total spent amount for all categories
                    spentAmount = await _context.Expenses
                        .Where(e => e.UserId == budget.UserId &&
                                   e.ExpenseDate >= budget.StartDate &&
                                   e.ExpenseDate <= budget.EndDate)
                        .SumAsync(e => e.Amount);
                }

                budget.SpentAmount = spentAmount;
                budget.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating spent amount for budget {BudgetId}", budgetId);
                throw;
            }
        }

        public async Task UpdateAllSpentAmountsAsync(long userId)
        {
            try
            {
                var budgets = await _context.Budgets
                    .Where(b => b.UserId == userId && b.IsActive)
                    .ToListAsync();

                foreach (var budget in budgets)
                {
                    await UpdateSpentAmountAsync(budget.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all spent amounts for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<BudgetDto>> GetOverBudgetCategoriesAsync(long userId)
        {
            try
            {
                var overBudgetCategories = await _context.Budgets
                    .Where(b => b.UserId == userId &&
                               b.IsActive &&
                               b.SpentAmount > b.BudgetAmount)
                    .Include(b => b.Category)
                    .Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        CategoryId = b.CategoryId,
                        CategoryName = b.Category != null ? b.Category.Name : "Tổng quát",
                        BudgetAmount = b.BudgetAmount,
                        SpentAmount = b.SpentAmount,
                        RemainingAmount = b.BudgetAmount - b.SpentAmount,
                        PercentageUsed = b.BudgetAmount > 0 ? (b.SpentAmount / b.BudgetAmount) * 100 : 0,
                        Currency = b.Currency,
                        PeriodType = b.PeriodType,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .OrderByDescending(b => b.PercentageUsed)
                    .ToListAsync();

                return overBudgetCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving over budget categories for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetBudgetUtilizationAsync(long userId, long? categoryId = null)
        {
            try
            {
                var query = _context.Budgets.Where(b => b.UserId == userId && b.IsActive);

                if (categoryId.HasValue)
                {
                    query = query.Where(b => b.CategoryId == categoryId.Value);
                }

                var budgets = await query.ToListAsync();

                if (!budgets.Any())
                {
                    return 0;
                }

                var totalBudget = budgets.Sum(b => b.BudgetAmount);
                var totalSpent = budgets.Sum(b => b.SpentAmount);

                return totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating budget utilization for user {UserId}", userId);
                throw;
            }
        }
    }
}
