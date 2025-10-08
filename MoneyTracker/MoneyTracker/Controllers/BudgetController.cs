using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    public class BudgetController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(ExpenseManagerContext context, ILogger<BudgetController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgets([FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

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

                return Ok(budgets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budgets");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

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

                if (budget == null)
                {
                    return NotFound();
                }

                return Ok(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget {BudgetId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

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
                    return BadRequest(new { message = "Budget already exists for this category and period" });
                }

                var budget = new Budget
                {
                    UserId = userId.Value,
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
                await UpdateSpentAmountInternal(budget.Id);

                _logger.LogInformation("Budget created successfully for user {UserId}", userId);
                return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(long id, [FromBody] UpdateBudgetDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (budget == null)
                {
                    return NotFound();
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
                await UpdateSpentAmountInternal(budget.Id);

                _logger.LogInformation("Budget {BudgetId} updated successfully for user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget {BudgetId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (budget == null)
                {
                    return NotFound();
                }

                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Budget {BudgetId} deleted successfully for user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget {BudgetId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetBudgetSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

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

                var summary = new BudgetSummaryDto
                {
                    TotalBudget = totalBudget,
                    TotalSpent = totalSpent,
                    TotalRemaining = totalRemaining,
                    PercentageUsed = percentageUsed,
                    ActiveBudgets = activeBudgets.Count,
                    OverBudgetCategories = overBudgetCategories,
                    Budgets = budgetDtos
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget summary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/update-spent")]
        public async Task<IActionResult> UpdateSpentAmount(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (budget == null)
                {
                    return NotFound();
                }

                await UpdateSpentAmountInternal(id);

                return Ok(new { message = "Spent amount updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating spent amount for budget {BudgetId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private async Task UpdateSpentAmountInternal(long budgetId)
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

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            return null;
        }
    }
}
