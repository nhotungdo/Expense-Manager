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
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ExpenseManagerContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(u => u.Username.Contains(filter.SearchTerm) ||
                                        u.Email.Contains(filter.SearchTerm) ||
                                        (u.FullName != null && u.FullName.Contains(filter.SearchTerm)));
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {
                query = query.Where(u => u.Role == filter.Role.ToUpper());
            }

            if (filter.Enabled.HasValue)
            {
                query = query.Where(u => u.Enabled == filter.Enabled.Value);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    PictureUrl = u.PictureUrl,
                    Role = u.Role,
                    Enabled = u.Enabled,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PictureUrl = user.PictureUrl,
                Role = user.Role,
                Enabled = user.Enabled,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(userDto);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = updateUserDto.Username;
            user.FullName = updateUserDto.FullName;
            user.PictureUrl = updateUserDto.PictureUrl;
            user.Role = updateUserDto.Role.ToUpper();
            user.Enabled = updateUserDto.Enabled;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} updated user {UserId}", currentUserId, user.Id);

            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == id)
            {
                return BadRequest("Cannot delete your own account");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} deleted user {UserId}", currentUserId, user.Id);

            return NoContent();
        }

        [HttpGet("global-categories")]
        public async Task<IActionResult> GetGlobalCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == null)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    UserId = 0,
                    IsGlobal = true,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("global-categories")]
        public async Task<IActionResult> CreateGlobalCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate type
            if (createCategoryDto.Type.ToUpper() != "EXPENSE" && createCategoryDto.Type.ToUpper() != "INCOME")
            {
                return BadRequest("Type must be either 'EXPENSE' or 'INCOME'");
            }

            // Check if global category with same name exists
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == createCategoryDto.Name.ToLower() &&
                                         c.Type.ToUpper() == createCategoryDto.Type.ToUpper() &&
                                         c.UserId == null);

            if (existingCategory != null)
            {
                return BadRequest("Global category with this name already exists");
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Type = createCategoryDto.Type.ToUpper(),
                UserId = null, // Global category
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} created global category {CategoryId}", currentUserId, category.Id);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                UserId = 0,
                IsGlobal = true,
                CreatedAt = category.CreatedAt ?? DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetGlobalCategories), categoryDto);
        }

        [HttpPut("global-categories/{id}")]
        public async Task<IActionResult> UpdateGlobalCategory(long id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == null);

            if (category == null)
            {
                return NotFound("Global category not found");
            }

            // Validate type
            if (updateCategoryDto.Type.ToUpper() != "EXPENSE" && updateCategoryDto.Type.ToUpper() != "INCOME")
            {
                return BadRequest("Type must be either 'EXPENSE' or 'INCOME'");
            }

            category.Name = updateCategoryDto.Name;
            category.Type = updateCategoryDto.Type.ToUpper();

            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} updated global category {CategoryId}", currentUserId, category.Id);

            return NoContent();
        }

        [HttpDelete("global-categories/{id}")]
        public async Task<IActionResult> DeleteGlobalCategory(long id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == null);

            if (category == null)
            {
                return NotFound("Global category not found");
            }

            // Check if category is being used
            var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == id);
            var hasIncomes = await _context.Incomes.AnyAsync(i => i.CategoryId == id);

            if (hasExpenses || hasIncomes)
            {
                return BadRequest("Cannot delete global category that is being used");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} deleted global category {CategoryId}", currentUserId, category.Id);

            return NoContent();
        }

        [HttpGet("ai-suggestions")]
        public async Task<IActionResult> GetAiSuggestions([FromQuery] AiSuggestionFilterDto filter)
        {
            var query = _context.AiSuggestions.Include(a => a.User).AsQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
            {
                query = query.Where(a => a.UserId == filter.UserId.Value);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.CreatedTo.Value);
            }

            var suggestions = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AiSuggestionDto
                {
                    Id = a.Id,
                    Suggestion = a.Suggestion,
                    UserId = a.UserId,
                    CreatedAt = a.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetSystemStatistics()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.Enabled);
            var totalExpenses = await _context.Expenses.CountAsync();
            var totalIncomes = await _context.Incomes.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var globalCategories = await _context.Categories.CountAsync(c => c.UserId == null);

            var stats = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalExpenses = totalExpenses,
                TotalIncomes = totalIncomes,
                TotalCategories = totalCategories,
                GlobalCategories = globalCategories,
                UserRegistrationsByMonth = await GetUserRegistrationsByMonth()
            };

            return Ok(stats);
        }

        private async Task<Dictionary<string, int>> GetUserRegistrationsByMonth()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            var registrations = await _context.Users
                .Where(u => u.CreatedAt >= sixMonthsAgo)
                .GroupBy(u => new { u.CreatedAt!.Value.Year, u.CreatedAt.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            return registrations.ToDictionary(
                r => $"{r.Year}-{r.Month:D2}",
                r => r.Count
            );
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
