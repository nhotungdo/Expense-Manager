using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(ExpenseManagerContext context, ILogger<ExpenseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenses([FromQuery] ExpenseFilterDto filter)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= DateOnly.FromDateTime(filter.StartDate.Value));
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= DateOnly.FromDateTime(filter.EndDate.Value));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(e => e.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(e => e.Amount <= filter.MaxAmount.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(e => e.Note!.Contains(filter.SearchTerm) ||
                                        e.Category!.Name.Contains(filter.SearchTerm));
            }

            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Currency = e.Currency ?? "VND",
                    ExpenseDate = e.ExpenseDate,
                    Note = e.Note,
                    CategoryId = e.CategoryId ?? 0,
                    CategoryName = e.Category != null ? e.Category.Name : "Uncategorized",
                    UserId = e.UserId,
                    CreatedAt = e.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpense(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency ?? "VND",
                ExpenseDate = expense.ExpenseDate,
                Note = expense.Note,
                CategoryId = expense.CategoryId ?? 0,
                CategoryName = expense.Category?.Name ?? "Uncategorized",
                UserId = expense.UserId,
                CreatedAt = expense.CreatedAt ?? DateTime.UtcNow
            };

            return Ok(expenseDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto createExpenseDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate category exists and belongs to user or is global
            if (createExpenseDto.CategoryId > 0)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == createExpenseDto.CategoryId &&
                                            (c.UserId == userId || c.UserId == null) &&
                                            c.Type == "EXPENSE");

                if (category == null)
                {
                    return BadRequest("Invalid category");
                }
            }

            var expense = new Expense
            {
                UserId = userId.Value,
                CategoryId = createExpenseDto.CategoryId > 0 ? createExpenseDto.CategoryId : null,
                Amount = createExpenseDto.Amount,
                Currency = createExpenseDto.Currency,
                Note = createExpenseDto.Note,
                ExpenseDate = createExpenseDto.ExpenseDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} created expense {ExpenseId}", userId, expense.Id);

            // Return the created expense with category info
            await _context.Entry(expense).Reference(e => e.Category).LoadAsync();

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency ?? "VND",
                ExpenseDate = expense.ExpenseDate,
                Note = expense.Note,
                CategoryId = expense.CategoryId ?? 0,
                CategoryName = expense.Category?.Name ?? "Uncategorized",
                UserId = expense.UserId,
                CreatedAt = expense.CreatedAt ?? DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expenseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(long id, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            // Validate category exists and belongs to user or is global
            if (updateExpenseDto.CategoryId > 0)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == updateExpenseDto.CategoryId &&
                                            (c.UserId == userId || c.UserId == null) &&
                                            c.Type == "EXPENSE");

                if (category == null)
                {
                    return BadRequest("Invalid category");
                }
            }

            expense.CategoryId = updateExpenseDto.CategoryId > 0 ? updateExpenseDto.CategoryId : null;
            expense.Amount = updateExpenseDto.Amount;
            expense.Currency = updateExpenseDto.Currency;
            expense.Note = updateExpenseDto.Note;
            expense.ExpenseDate = updateExpenseDto.ExpenseDate;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated expense {ExpenseId}", userId, expense.Id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted expense {ExpenseId}", userId, expense.Id);

            return NoContent();
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
