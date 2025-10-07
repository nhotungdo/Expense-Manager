using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync(long userId, string? type = null);
        Task<Category?> GetCategoryByIdAsync(long id, long userId);
        Task<Category> CreateCategoryAsync(CategoryDto categoryDto, long userId);
        Task<Category?> UpdateCategoryAsync(long id, CategoryDto categoryDto, long userId);
        Task<bool> DeleteCategoryAsync(long id, long userId);
        Task<IEnumerable<Category>> GetDefaultCategoriesAsync();
        Task<bool> InitializeDefaultCategoriesAsync(long userId);
        Task<Dictionary<string, int>> GetCategoryUsageStatsAsync(long userId);
    }
}
