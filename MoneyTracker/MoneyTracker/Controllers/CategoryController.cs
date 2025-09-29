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
    public class CategoryController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ExpenseManagerContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] string? type = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Categories
                .Where(c => c.UserId == userId || c.UserId == null); // User's categories + global categories

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type.ToUpper() == type.ToUpper());
            }

            var categories = await query
                .OrderBy(c => c.UserId == null ? 0 : 1) // Global categories first
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    UserId = c.UserId ?? 0,
                    IsGlobal = c.UserId == null,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("global")]
        public async Task<IActionResult> GetGlobalCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == null)
                .OrderBy(c => c.Name)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && (c.UserId == userId || c.UserId == null));

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                UserId = category.UserId ?? 0,
                IsGlobal = category.UserId == null,
                CreatedAt = category.CreatedAt ?? DateTime.UtcNow
            };

            return Ok(categoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate type
            if (createCategoryDto.Type.ToUpper() != "EXPENSE" && createCategoryDto.Type.ToUpper() != "INCOME")
            {
                return BadRequest("Type must be either 'EXPENSE' or 'INCOME'");
            }

            // Check if category name already exists for this user
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == createCategoryDto.Name.ToLower() &&
                                         c.Type.ToUpper() == createCategoryDto.Type.ToUpper() &&
                                         c.UserId == userId);

            if (existingCategory != null)
            {
                return BadRequest("Category with this name already exists");
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Type = createCategoryDto.Type.ToUpper(),
                UserId = createCategoryDto.IsGlobal ? null : userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} created category {CategoryId}", userId, category.Id);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                UserId = category.UserId ?? 0,
                IsGlobal = category.UserId == null,
                CreatedAt = category.CreatedAt ?? DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound("Category not found or you don't have permission to edit it");
            }

            // Validate type
            if (updateCategoryDto.Type.ToUpper() != "EXPENSE" && updateCategoryDto.Type.ToUpper() != "INCOME")
            {
                return BadRequest("Type must be either 'EXPENSE' or 'INCOME'");
            }

            // Check if category name already exists for this user (excluding current category)
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == updateCategoryDto.Name.ToLower() &&
                                         c.Type.ToUpper() == updateCategoryDto.Type.ToUpper() &&
                                         c.UserId == userId &&
                                         c.Id != id);

            if (existingCategory != null)
            {
                return BadRequest("Category with this name already exists");
            }

            category.Name = updateCategoryDto.Name;
            category.Type = updateCategoryDto.Type.ToUpper();

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated category {CategoryId}", userId, category.Id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound("Category not found or you don't have permission to delete it");
            }

            // Check if category is being used
            var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == id);
            var hasIncomes = await _context.Incomes.AnyAsync(i => i.CategoryId == id);

            if (hasExpenses || hasIncomes)
            {
                return BadRequest("Cannot delete category that is being used by expenses or incomes");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted category {CategoryId}", userId, category.Id);

            return NoContent();
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
