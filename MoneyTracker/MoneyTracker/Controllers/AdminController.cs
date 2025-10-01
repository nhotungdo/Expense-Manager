using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/admin")]
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

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetAdminDashboardData()
        {
            try
            {
                // Get total users
                var totalUsers = await _context.Users.CountAsync();

                // Get active users (enabled = true)
                var activeUsers = await _context.Users.CountAsync(u => u.Enabled);

                // Get new users in last 7 days
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var newUsers = await _context.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo);

                // Get total AI suggestions
                var totalAISuggestions = await _context.AiSuggestions.CountAsync();

                // Get user registration data for last 6 months
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var userRegistrationData = await _context.Users
                    .Where(u => u.CreatedAt.HasValue && u.CreatedAt >= sixMonthsAgo)
                    .GroupBy(u => new { u.CreatedAt!.Value.Year, u.CreatedAt!.Value.Month })
                    .Select(g => new
                    {
                        month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        count = g.Count()
                    })
                    .OrderBy(x => x.month)
                    .ToListAsync();

                // Get system activity data
                var systemActivityData = new[]
                {
                    new { type = "Tổng giao dịch", count = await _context.Expenses.CountAsync() + await _context.Incomes.CountAsync() },
                    new { type = "Danh mục", count = await _context.Categories.CountAsync() },
                    new { type = "Gợi ý AI", count = await _context.AiSuggestions.CountAsync() },
                    new { type = "Email gửi", count = await _context.Emails.CountAsync() }
                };

                var dashboardData = new
                {
                    totalUsers,
                    activeUsers,
                    newUsers,
                    totalAISuggestions,
                    userRegistrationData,
                    systemActivityData
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard data");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? enabled = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.Username.Contains(searchTerm) ||
                                           u.Email.Contains(searchTerm) ||
                                           (u.FullName != null && u.FullName.Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                if (enabled.HasValue)
                {
                    query = query.Where(u => u.Enabled == enabled.Value);
                }

                if (createdFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= createdFrom.Value);
                }

                if (createdTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= createdTo.Value);
                }

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(limit)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        Role = u.Role,
                        Enabled = u.Enabled,
                        PictureUrl = u.PictureUrl,
                        LastLogin = u.LastLogin,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(long id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        Role = u.Role,
                        Enabled = u.Enabled,
                        PictureUrl = u.PictureUrl,
                        LastLogin = u.LastLogin,
                        CreatedAt = u.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserDto userDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                user.Username = userDto.Username;
                user.FullName = userDto.FullName;
                user.Role = userDto.Role;
                user.Enabled = userDto.Enabled;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user {UserId} by admin {AdminId}", id, GetCurrentUserId());

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Check if user has transactions
                var hasTransactions = await _context.Expenses.AnyAsync(e => e.UserId == id) ||
                                    await _context.Incomes.AnyAsync(i => i.UserId == id);

                if (hasTransactions)
                {
                    return BadRequest("Cannot delete user with existing transactions. Disable the user instead.");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted user {UserId} by admin {AdminId}", id, GetCurrentUserId());

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetGlobalCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = c.Type,
                        Description = c.Description,
                        UserId = c.UserId ?? 0,
                        IsGlobal = c.UserId == null,
                        CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting global categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateGlobalCategory([FromBody] CreateCategoryDto categoryDto)
        {
            try
            {
                var category = new Category
                {
                    Name = categoryDto.Name,
                    Type = categoryDto.Type,
                    Description = categoryDto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created global category {CategoryName} by admin {AdminId}",
                    categoryDto.Name, GetCurrentUserId());

                return Ok(new { message = "Category created successfully", categoryId = category.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating global category");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateGlobalCategory(long id, [FromBody] UpdateCategoryDto categoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found");
                }

                category.Name = categoryDto.Name;
                category.Type = categoryDto.Type;
                category.Description = categoryDto.Description;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated global category {CategoryId} by admin {AdminId}",
                    id, GetCurrentUserId());

                return Ok(new { message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating global category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteGlobalCategory(long id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found");
                }

                // Check if category is being used
                var isUsed = await _context.Expenses.AnyAsync(e => e.CategoryId == id) ||
                           await _context.Incomes.AnyAsync(i => i.CategoryId == id);

                if (isUsed)
                {
                    return BadRequest("Cannot delete category that is being used by transactions");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted global category {CategoryId} by admin {AdminId}",
                    id, GetCurrentUserId());

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting global category {CategoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ai-suggestions")]
        public async Task<IActionResult> GetAISuggestions([FromQuery] int limit = 10)
        {
            try
            {
                var suggestions = await _context.AiSuggestions
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .Select(a => new AiSuggestionDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        UserName = a.User.Username,
                        Suggestion = a.Suggestion,
                        CreatedAt = a.CreatedAt ?? DateTime.UtcNow
                    })
                    .ToListAsync();

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI suggestions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("system-logs")]
        public IActionResult GetSystemLogs([FromQuery] int limit = 50)
        {
            try
            {
                // This is a simplified version. In a real application, you would have a proper logging system
                var logs = new List<object>
                {
                    new { timestamp = DateTime.UtcNow.AddHours(-1), type = "INFO", description = "User logged in" },
                    new { timestamp = DateTime.UtcNow.AddHours(-2), type = "INFO", description = "New expense added" },
                    new { timestamp = DateTime.UtcNow.AddHours(-3), type = "WARNING", description = "High spending detected" },
                    new { timestamp = DateTime.UtcNow.AddHours(-4), type = "INFO", description = "AI suggestion generated" },
                    new { timestamp = DateTime.UtcNow.AddHours(-5), type = "SUCCESS", description = "User registration completed" }
                };

                return Ok(logs.Take(limit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system logs");
                return StatusCode(500, "Internal server error");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}