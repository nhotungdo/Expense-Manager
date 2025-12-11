using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing categories with multi-level hierarchy
/// Handles CRUD operations, parent-child relationships, and default categories
/// </summary>
public interface ICategoryService
{
    Task<CategoryResponseDto?> GetCategoryByIdAsync(long categoryId, long userId);
    Task<List<CategoryResponseDto>> GetUserCategoriesAsync(long userId, int? type = null);
    Task<List<CategoryResponseDto>> GetCategoryTreeAsync(long userId, int? type = null);
    Task<List<CategorySummaryDto>> GetCategorySummariesAsync(long userId, int? type = null);
    Task<CategoryResponseDto> CreateCategoryAsync(long userId, CreateCategoryDto dto);
    Task<CategoryResponseDto> UpdateCategoryAsync(long userId, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(long categoryId, long userId);
    Task<bool> DeactivateCategoryAsync(long categoryId, long userId);
    Task InitializeDefaultCategoriesAsync(long userId);
}

public class CategoryService : ICategoryService
{
    private readonly ExpenseManagerContext _context;

    public CategoryService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(long categoryId, long userId)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .Where(c => c.Id == categoryId && (c.UserId == userId || c.IsDefault))
            .FirstOrDefaultAsync();

        if (category == null)
            return null;

        return await MapToResponseDtoAsync(category, userId);
    }

    /// <summary>
    /// Get all categories for a user (flat list)
    /// </summary>
    public async Task<List<CategoryResponseDto>> GetUserCategoriesAsync(long userId, int? type = null)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .Where(c => (c.UserId == userId || c.IsDefault) && c.IsActive);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        var result = new List<CategoryResponseDto>();
        foreach (var category in categories)
        {
            result.Add(await MapToResponseDtoAsync(category, userId));
        }

        return result;
    }

    /// <summary>
    /// Get categories in tree structure (parent-child hierarchy)
    /// </summary>
    public async Task<List<CategoryResponseDto>> GetCategoryTreeAsync(long userId, int? type = null)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .Where(c => (c.UserId == userId || c.IsDefault) && c.IsActive);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var allCategories = await query.ToListAsync();

        // Get root categories (no parent)
        var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

        var result = new List<CategoryResponseDto>();
        foreach (var rootCategory in rootCategories)
        {
            var dto = await MapToResponseDtoAsync(rootCategory, userId);
            dto.SubCategories = await GetSubCategoriesAsync(rootCategory.Id, allCategories, userId);
            result.Add(dto);
        }

        return result.OrderBy(c => c.Name).ToList();
    }

    /// <summary>
    /// Get category summaries (minimal info for dropdowns)
    /// </summary>
    public async Task<List<CategorySummaryDto>> GetCategorySummariesAsync(long userId, int? type = null)
    {
        var query = _context.Categories
            .Where(c => (c.UserId == userId || c.IsDefault) && c.IsActive);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new CategorySummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type,
            Icon = c.Icon,
            Color = c.Color,
            IsDefault = c.IsDefault
        }).ToList();
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    public async Task<CategoryResponseDto> CreateCategoryAsync(long userId, CreateCategoryDto dto)
    {
        // Verify parent category if specified
        if (dto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _context.Categories
                .Where(c => c.Id == dto.ParentCategoryId.Value && (c.UserId == userId || c.IsDefault))
                .FirstOrDefaultAsync();

            if (parentCategory == null)
                throw new InvalidOperationException("Parent category not found");

            // Ensure parent and child have same type
            if (parentCategory.Type != dto.Type)
                throw new InvalidOperationException("Parent and child categories must have the same type");
        }

        var category = new Category
        {
            ParentCategoryId = dto.ParentCategoryId,
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            Icon = dto.Icon,
            Color = dto.Color,
            UserId = userId,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(category, userId);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    public async Task<CategoryResponseDto> UpdateCategoryAsync(long userId, UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .Where(c => c.Id == dto.Id && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (category == null)
            throw new InvalidOperationException("Category not found or you don't have permission");

        if (category.IsDefault)
            throw new InvalidOperationException("Cannot modify default categories");

        // Update fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            category.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            category.Description = dto.Description;

        if (!string.IsNullOrWhiteSpace(dto.Icon))
            category.Icon = dto.Icon;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            category.Color = dto.Color;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(category, userId);
    }

    /// <summary>
    /// Delete a category (only if no transactions)
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(long categoryId, long userId)
    {
        var category = await _context.Categories
            .Include(c => c.Transactions)
            .Include(c => c.InverseParentCategory)
            .Where(c => c.Id == categoryId && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (category == null)
            return false;

        if (category.IsDefault)
            throw new InvalidOperationException("Cannot delete default categories");

        // Check if category has transactions
        if (category.Transactions.Any())
            throw new InvalidOperationException("Cannot delete category with existing transactions");

        // Check if category has subcategories
        if (category.InverseParentCategory.Any())
            throw new InvalidOperationException("Cannot delete category with subcategories");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Deactivate a category (soft delete)
    /// </summary>
    public async Task<bool> DeactivateCategoryAsync(long categoryId, long userId)
    {
        var category = await _context.Categories
            .Where(c => c.Id == categoryId && c.UserId == userId)
            .FirstOrDefaultAsync();

        if (category == null)
            return false;

        if (category.IsDefault)
            throw new InvalidOperationException("Cannot deactivate default categories");

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Initialize default categories for a new user
    /// </summary>
    public async Task InitializeDefaultCategoriesAsync(long userId)
    {
        // Check if user already has categories
        var hasCategories = await _context.Categories
            .AnyAsync(c => c.UserId == userId);

        if (hasCategories)
            return;

        var defaultCategories = new List<Category>
        {
            // Income Categories
            new Category { Name = "Lương", Type = 1, Icon = "💰", Color = "#4CAF50", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Bán thời gian", Type = 1, Icon = "💼", Color = "#2196F3", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Đầu tư", Type = 1, Icon = "📈", Color = "#9C27B0", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Quà tặng", Type = 1, Icon = "🎁", Color = "#FF9800", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Thu nhập khác", Type = 1, Icon = "💵", Color = "#607D8B", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Expense Categories
            new Category { Name = "Ăn uống", Type = 2, Icon = "🍔", Color = "#FF5722", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Di chuyển", Type = 2, Icon = "🚗", Color = "#3F51B5", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Mua sắm", Type = 2, Icon = "🛍️", Color = "#E91E63", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Giải trí", Type = 2, Icon = "🎬", Color = "#9C27B0", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Hóa đơn & Tiện ích", Type = 2, Icon = "📄", Color = "#FF9800", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Sức khỏe", Type = 2, Icon = "🏥", Color = "#F44336", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Giáo dục", Type = 2, Icon = "📚", Color = "#2196F3", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Chi tiêu khác", Type = 2, Icon = "💸", Color = "#607D8B", UserId = userId, IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _context.Categories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();
    }

    // Helper Methods

    private async Task<CategoryResponseDto> MapToResponseDtoAsync(Category category, long userId)
    {
        var transactionCount = await _context.Transactions
            .CountAsync(t => t.CategoryId == category.Id && t.UserId == userId);

        return new CategoryResponseDto
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            Name = category.Name,
            Type = category.Type,
            TypeDisplay = category.Type == 1 ? "Thu nhập" : "Chi tiêu",
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            UserId = category.UserId,
            IsDefault = category.IsDefault,
            IsActive = category.IsActive,
            TransactionCount = transactionCount,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private async Task<List<CategoryResponseDto>> GetSubCategoriesAsync(long parentId, List<Category> allCategories, long userId)
    {
        var subCategories = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();
        var result = new List<CategoryResponseDto>();

        foreach (var subCategory in subCategories)
        {
            var dto = await MapToResponseDtoAsync(subCategory, userId);
            dto.SubCategories = await GetSubCategoriesAsync(subCategory.Id, allCategories, userId);
            result.Add(dto);
        }

        return result.OrderBy(c => c.Name).ToList();
    }
}
