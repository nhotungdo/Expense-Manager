using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Category;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var userCategories = await _categoryService.GetUserCategoriesAsync(userId.Value);
            var systemCategories = await _categoryService.GetSystemCategoriesAsync();

            var allCategories = userCategories.Concat(systemCategories).Select(MapToDto);
            return Ok(allCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id, userId.Value);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            return Ok(MapToDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var category = new Category
            {
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                Icon = request.Icon,
                Color = request.Color,
                UserId = userId.Value
            };

            var createdCategory = await _categoryService.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, MapToDto(createdCategory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(long id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id, userId.Value);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            // Only allow updating user's own categories
            if (category.UserId != userId.Value)
            {
                return Forbid("Cannot update system categories");
            }

            category.Name = request.Name;
            category.Type = request.Type;
            category.Description = request.Description;
            category.Icon = request.Icon;
            category.Color = request.Color;

            var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
            return Ok(MapToDto(updatedCategory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _categoryService.DeleteCategoryAsync(id, userId.Value);
            if (!success)
            {
                return NotFound("Category not found or cannot be deleted");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
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

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            UserId = category.UserId,
            IsDefault = category.IsDefault,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
