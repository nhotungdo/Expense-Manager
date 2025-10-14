using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetUserCategoriesAsync(long userId)
    {
        return await _unitOfWork.Categories.FindAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Category>> GetSystemCategoriesAsync()
    {
        return await _unitOfWork.Categories.FindAsync(c => c.IsDefault && c.IsActive);
    }

    public async Task<Category?> GetCategoryByIdAsync(long id, long userId)
    {
        return await _unitOfWork.Categories.FirstOrDefaultAsync(c =>
            c.Id == id && (c.UserId == userId || c.IsDefault));
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.IsDefault = false;
        category.IsActive = true;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryId} for user {UserId}", category.Id, category.UserId);
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId} for user {UserId}", category.Id, category.UserId);
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(long id, long userId)
    {
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c =>
            c.Id == id && c.UserId == userId && !c.IsDefault);

        if (category == null)
        {
            return false;
        }

        // Check if category is in use
        if (await IsCategoryInUseAsync(id))
        {
            _logger.LogWarning("Cannot delete category {CategoryId} - it is in use", id);
            return false;
        }

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId} for user {UserId}", id, userId);
        return true;
    }

    public async Task<bool> IsCategoryInUseAsync(long categoryId)
    {
        var hasTransactions = await _unitOfWork.Transactions.ExistsAsync(t => t.CategoryId == categoryId);
        var hasBudgets = await _unitOfWork.Budgets.ExistsAsync(b => b.CategoryId == categoryId);

        return hasTransactions || hasBudgets;
    }
}
