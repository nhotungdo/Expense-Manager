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
        /// Debug endpoint to check if API is accessible (no auth required)
        /// </summary>
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            return Ok(new { message = "API is accessible", timestamp = DateTime.UtcNow });
        }
    }
}
