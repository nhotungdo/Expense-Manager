using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Budget;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(IBudgetService budgetService, ILogger<BudgetsController> logger)
    {
        _budgetService = budgetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgets()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var budgets = await _budgetService.GetUserBudgetsAsync(userId.Value);
            var budgetDtos = budgets.Select(MapToDto);
            return Ok(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetDto>> GetBudget(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var budget = await _budgetService.GetBudgetByIdAsync(id, userId.Value);
            if (budget == null)
            {
                return NotFound("Budget not found");
            }

            return Ok(MapToDto(budget));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var budget = new Budget
            {
                UserId = userId.Value,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Period = request.Period,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var createdBudget = await _budgetService.CreateBudgetAsync(budget);
            return CreatedAtAction(nameof(GetBudget), new { id = createdBudget.Id }, MapToDto(createdBudget));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BudgetDto>> UpdateBudget(long id, [FromBody] UpdateBudgetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var budget = await _budgetService.GetBudgetByIdAsync(id, userId.Value);
            if (budget == null)
            {
                return NotFound("Budget not found");
            }

            budget.CategoryId = request.CategoryId;
            budget.Amount = request.Amount;
            budget.Period = request.Period;
            budget.StartDate = request.StartDate;
            budget.EndDate = request.EndDate;

            var updatedBudget = await _budgetService.UpdateBudgetAsync(budget);
            return Ok(MapToDto(updatedBudget));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBudget(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _budgetService.DeleteBudgetAsync(id, userId.Value);
            if (!success)
            {
                return NotFound("Budget not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static BudgetDto MapToDto(Budget budget)
    {
        return new BudgetDto
        {
            Id = budget.Id,
            UserId = budget.UserId,
            CategoryId = budget.CategoryId,
            Amount = budget.Amount,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt,
            Category = budget.Category != null ? new CategoryDto
            {
                Id = budget.Category.Id,
                Name = budget.Category.Name,
                Type = budget.Category.Type,
                Description = budget.Category.Description,
                Icon = budget.Category.Icon,
                Color = budget.Category.Color,
                IsDefault = budget.Category.IsDefault
            } : null
        };
    }
}
