using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<TransactionController> _logger;
        private readonly IExpenseService _expenseService;
        private readonly IIncomeService _incomeService;

        public TransactionController(
            ExpenseManagerContext context,
            ILogger<TransactionController> logger,
            IExpenseService expenseService,
            IIncomeService incomeService)
        {
            _context = context;
            _logger = logger;
            _expenseService = expenseService;
            _incomeService = incomeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var query = _context.Transactions
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Category)
                    .AsQueryable();

                // Apply filters
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(filter.EndDate.Value));
                }

                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(filter.Type))
                {
                    query = query.Where(t => t.Type.ToLower() == filter.Type.ToLower());
                }

                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(t => t.Amount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(t => t.Note!.Contains(filter.SearchTerm));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "amount" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.Amount)
                        : query.OrderByDescending(t => t.Amount),
                    "category" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.Category!.Name)
                        : query.OrderByDescending(t => t.Category!.Name),
                    _ => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.TransactionDate)
                        : query.OrderByDescending(t => t.TransactionDate)
                };

                var totalCount = await query.CountAsync();
                var transactions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                var result = new
                {
                    transactions,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var transaction = await _context.Transactions
                    .Where(t => t.Id == id && t.UserId == userId)
                    .Include(t => t.Category)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return NotFound();
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var transaction = new Transaction
                {
                    UserId = userId.Value,
                    CategoryId = createDto.CategoryId,
                    Type = createDto.Type.ToLower(),
                    Amount = createDto.Amount,
                    Currency = createDto.Currency ?? "VND",
                    Note = createDto.Note,
                    TransactionDate = createDto.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Also create in the appropriate table (Expense or Income)
                if (createDto.Type.ToLower() == "expense")
                {
                    var expense = new Expense
                    {
                        UserId = userId.Value,
                        CategoryId = createDto.CategoryId,
                        Amount = createDto.Amount,
                        Currency = createDto.Currency ?? "VND",
                        Note = createDto.Note,
                        ExpenseDate = createDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Expenses.Add(expense);
                }
                else if (createDto.Type.ToLower() == "income")
                {
                    var income = new Income
                    {
                        UserId = userId.Value,
                        CategoryId = createDto.CategoryId,
                        Amount = createDto.Amount,
                        Currency = createDto.Currency ?? "VND",
                        Note = createDto.Note,
                        IncomeDate = createDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Incomes.Add(income);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction created successfully for user {UserId}", userId);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(long id, [FromBody] UpdateTransactionDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return NotFound();
                }

                transaction.CategoryId = updateDto.CategoryId;
                transaction.Type = updateDto.Type.ToLower();
                transaction.Amount = updateDto.Amount;
                transaction.Currency = updateDto.Currency ?? "VND";
                transaction.Note = updateDto.Note;
                transaction.TransactionDate = updateDto.TransactionDate;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} updated successfully for user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return NotFound();
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} deleted successfully for user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetTransactionSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var query = _context.Transactions.Where(t => t.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(startDate.Value));
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(endDate.Value));
                }

                var totalIncome = await query.Where(t => t.Type.ToLower() == "income").SumAsync(t => t.Amount);
                var totalExpense = await query.Where(t => t.Type.ToLower() == "expense").SumAsync(t => t.Amount);
                var totalTransactions = await query.CountAsync();
                var incomeTransactions = await query.Where(t => t.Type.ToLower() == "income").CountAsync();
                var expenseTransactions = await query.Where(t => t.Type.ToLower() == "expense").CountAsync();

                var recentTransactions = await query
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                var summary = new TransactionSummaryDto
                {
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    NetAmount = totalIncome - totalExpense,
                    TotalTransactions = totalTransactions,
                    IncomeTransactions = incomeTransactions,
                    ExpenseTransactions = expenseTransactions,
                    RecentTransactions = recentTransactions
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction summary");
                return StatusCode(500, new { message = "Internal server error" });
            }
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
