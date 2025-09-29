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
    public class IncomeController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(ExpenseManagerContext context, ILogger<IncomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetIncomes([FromQuery] IncomeFilterDto filter)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId);

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                query = query.Where(i => i.IncomeDate >= DateOnly.FromDateTime(filter.StartDate.Value));
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(i => i.IncomeDate <= DateOnly.FromDateTime(filter.EndDate.Value));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(i => i.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(i => i.Amount <= filter.MaxAmount.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(i => i.Note!.Contains(filter.SearchTerm) ||
                                        i.Category!.Name.Contains(filter.SearchTerm));
            }

            var incomes = await query
                .OrderByDescending(i => i.IncomeDate)
                .Select(i => new IncomeDto
                {
                    Id = i.Id,
                    Amount = i.Amount,
                    Currency = i.Currency ?? "VND",
                    IncomeDate = i.IncomeDate,
                    Note = i.Note,
                    CategoryId = i.CategoryId ?? 0,
                    CategoryName = i.Category != null ? i.Category.Name : "Uncategorized",
                    UserId = i.UserId,
                    CreatedAt = i.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncome(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var income = await _context.Incomes
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            var incomeDto = new IncomeDto
            {
                Id = income.Id,
                Amount = income.Amount,
                Currency = income.Currency ?? "VND",
                IncomeDate = income.IncomeDate,
                Note = income.Note,
                CategoryId = income.CategoryId ?? 0,
                CategoryName = income.Category?.Name ?? "Uncategorized",
                UserId = income.UserId,
                CreatedAt = income.CreatedAt ?? DateTime.UtcNow
            };

            return Ok(incomeDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncome([FromBody] CreateIncomeDto createIncomeDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate category exists and belongs to user or is global
            if (createIncomeDto.CategoryId > 0)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == createIncomeDto.CategoryId &&
                                            (c.UserId == userId || c.UserId == null) &&
                                            c.Type == "INCOME");

                if (category == null)
                {
                    return BadRequest("Invalid category");
                }
            }

            var income = new Income
            {
                UserId = userId.Value,
                CategoryId = createIncomeDto.CategoryId > 0 ? createIncomeDto.CategoryId : null,
                Amount = createIncomeDto.Amount,
                Currency = createIncomeDto.Currency,
                Note = createIncomeDto.Note,
                IncomeDate = createIncomeDto.IncomeDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} created income {IncomeId}", userId, income.Id);

            // Return the created income with category info
            await _context.Entry(income).Reference(i => i.Category).LoadAsync();

            var incomeDto = new IncomeDto
            {
                Id = income.Id,
                Amount = income.Amount,
                Currency = income.Currency ?? "VND",
                IncomeDate = income.IncomeDate,
                Note = income.Note,
                CategoryId = income.CategoryId ?? 0,
                CategoryName = income.Category?.Name ?? "Uncategorized",
                UserId = income.UserId,
                CreatedAt = income.CreatedAt ?? DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, incomeDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncome(long id, [FromBody] UpdateIncomeDto updateIncomeDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            // Validate category exists and belongs to user or is global
            if (updateIncomeDto.CategoryId > 0)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == updateIncomeDto.CategoryId &&
                                            (c.UserId == userId || c.UserId == null) &&
                                            c.Type == "INCOME");

                if (category == null)
                {
                    return BadRequest("Invalid category");
                }
            }

            income.CategoryId = updateIncomeDto.CategoryId > 0 ? updateIncomeDto.CategoryId : null;
            income.Amount = updateIncomeDto.Amount;
            income.Currency = updateIncomeDto.Currency;
            income.Note = updateIncomeDto.Note;
            income.IncomeDate = updateIncomeDto.IncomeDate;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated income {IncomeId}", userId, income.Id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncome(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted income {IncomeId}", userId, income.Id);

            return NoContent();
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
