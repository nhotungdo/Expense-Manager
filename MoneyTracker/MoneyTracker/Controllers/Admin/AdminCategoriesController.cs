using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Category;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminCategoriesController> _logger;

    public AdminCategoriesController(
        IUnitOfWork unitOfWork,
        ILogger<AdminCategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetSystemCategories()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetSystemCategoriesAsync();

            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description,
                Icon = c.Icon,
                Color = c.Color,
                UserId = c.UserId,
                IsDefault = c.IsDefault,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system categories");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateSystemCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = new Category
            {
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                Icon = request.Icon,
                Color = request.Color,
                UserId = null, // System category
                IsDefault = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = new CategoryDto
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

            _logger.LogInformation("System category {CategoryId} created", category.Id);
            return CreatedAtAction(nameof(GetSystemCategories), new { id = category.Id }, categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system category");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateSystemCategory(long id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null || category.UserId != null)
            {
                return NotFound("System category not found");
            }

            category.Name = request.Name;
            category.Type = request.Type;
            category.Description = request.Description;
            category.Icon = request.Icon;
            category.Color = request.Color;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = new CategoryDto
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

            _logger.LogInformation("System category {CategoryId} updated", id);
            return Ok(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system category");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSystemCategory(long id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null || category.UserId != null)
            {
                return NotFound("System category not found");
            }

            // Check if category is being used
            var isUsed = await _unitOfWork.Transactions.IsCategoryUsedAsync(id);
            if (isUsed)
            {
                return BadRequest("Cannot delete category that is being used");
            }

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("System category {CategoryId} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system category");
            return StatusCode(500, "Internal server error");
        }
    }
}
