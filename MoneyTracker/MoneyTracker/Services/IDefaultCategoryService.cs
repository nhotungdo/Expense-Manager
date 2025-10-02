using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public interface IDefaultCategoryService
    {
        Task SetupDefaultCategoriesAsync(long userId);
        Task<List<Category>> GetDefaultExpenseCategoriesAsync();
        Task<List<Category>> GetDefaultIncomeCategoriesAsync();
        Task<bool> HasUserCategoriesAsync(long userId);
    }
}
