using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetUserCategoriesAsync(long userId);
    Task<IEnumerable<Category>> GetSystemCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(long id, long userId);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(long id, long userId);
    Task<bool> IsCategoryInUseAsync(long categoryId);
}
