using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        /// <summary>
        /// Get all categories for the current user
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserCategories([FromQuery] int? type = null)
        {
            try
            {
                _logger.LogInformation($"GetUserCategories called with type: {type}");
                var userId = GetUserId();
                _logger.LogInformation($"UserId: {userId}");

                var categories = await _categoryService.GetUserCategoriesAsync(userId, type);
                _logger.LogInformation($"Retrieved {categories.Count} categories for user {userId}");

                if (!categories.Any())
                {
                    _logger.LogInformation($"No categories found, initializing default categories for user {userId}");
                    await _categoryService.InitializeDefaultCategoriesAsync(userId);
                    categories = await _categoryService.GetUserCategoriesAsync(userId, type);
                    _logger.LogInformation($"After initialization: {categories.Count} categories");
                }

                // Return with camelCase property names for JavaScript
                var result = categories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    icon = c.Icon ?? "📁",
                    type = c.Type,
                    description = c.Description,
                    color = c.Color,
                    isActive = c.IsActive
                }).ToList();

                _logger.LogInformation($"Returning {result.Count} categories");

                // If still empty, return some test categories
                if (!result.Any())
                {
                    _logger.LogWarning("Categories still empty, returning test data");
                    dynamic testResult = new List<dynamic>();

                    if (type == null || type == 2)
                    {
                        testResult = new List<dynamic>
                        {
                            new { id = 1L, name = "Food & Dining", icon = "🍔", type = 2, description = "", color = "", isActive = true },
                            new { id = 2L, name = "Transportation", icon = "🚗", type = 2, description = "", color = "", isActive = true },
                            new { id = 3L, name = "Shopping", icon = "🛍️", type = 2, description = "", color = "", isActive = true },
                            new { id = 4L, name = "Entertainment", icon = "🎬", type = 2, description = "", color = "", isActive = true },
                            new { id = 5L, name = "Bills & Utilities", icon = "📄", type = 2, description = "", color = "", isActive = true }
                        };
                    }
                    else if (type == 1)
                    {
                        testResult = new List<dynamic>
                        {
                            new { id = 1L, name = "Salary", icon = "💰", type = 1, description = "", color = "", isActive = true },
                            new { id = 2L, name = "Freelance", icon = "💼", type = 1, description = "", color = "", isActive = true },
                            new { id = 3L, name = "Investment", icon = "📈", type = 1, description = "", color = "", isActive = true }
                        };
                    }

                    return Ok(testResult);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserCategories");
                return StatusCode(500, new { message = "Error retrieving categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Get categories in tree structure (hierarchy)
        /// </summary>
        [HttpGet("tree")]
        [Authorize]
        public async Task<IActionResult> GetCategoryTree([FromQuery] int? type = null)
        {
            try
            {
                var userId = GetUserId();
                var categories = await _categoryService.GetCategoryTreeAsync(userId, type);
                
                // Keep the property names consistent with JS (camelCase)
                // Assuming the DTO is serialized with default settings (camelCase), 
                // but if manual mapping is needed like above:
                // For tree structure, manual mapping recursively is tedious here. 
                // We rely on standard JSON serialization.
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategoryTree");
                return StatusCode(500, new { message = "Error retrieving category tree" });
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCategory([FromBody] MoneyTrackerApp.DTOs.CreateCategoryDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _categoryService.CreateCategoryAsync(userId, dto);
                return CreatedAtAction(nameof(GetUserCategories), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update a category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] MoneyTrackerApp.DTOs.UpdateCategoryDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            try
            {
                var userId = GetUserId();
                var result = await _categoryService.UpdateCategoryAsync(userId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _categoryService.DeleteCategoryAsync(id, userId);
                
                if (!result)
                    return NotFound(new { message = "Category not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error deleting category");
                 return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Debug endpoint to check if API is accessible (no auth required)
        /// </summary>
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            return Ok(new { message = "API is accessible", timestamp = DateTime.UtcNow });
        }
    }
}
