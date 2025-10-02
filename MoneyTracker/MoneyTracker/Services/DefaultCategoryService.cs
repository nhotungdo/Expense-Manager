using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class DefaultCategoryService : IDefaultCategoryService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<DefaultCategoryService> _logger;

        public DefaultCategoryService(ExpenseManagerContext context, ILogger<DefaultCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasUserCategoriesAsync(long userId)
        {
            return await _context.Categories.AnyAsync(c => c.UserId == userId);
        }

        public async Task SetupDefaultCategoriesAsync(long userId)
        {
            try
            {
                // Check if user already has categories
                if (await HasUserCategoriesAsync(userId))
                {
                    _logger.LogInformation("User {UserId} already has categories, skipping default setup", userId);
                    return;
                }

                var defaultExpenseCategories = await GetDefaultExpenseCategoriesAsync();
                var defaultIncomeCategories = await GetDefaultIncomeCategoriesAsync();

                // Create user-specific categories based on defaults
                var userCategories = new List<Category>();

                foreach (var category in defaultExpenseCategories)
                {
                    userCategories.Add(new Category
                    {
                        Name = category.Name,
                        Type = category.Type,
                        Description = category.Description,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var category in defaultIncomeCategories)
                {
                    userCategories.Add(new Category
                    {
                        Name = category.Name,
                        Type = category.Type,
                        Description = category.Description,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.Categories.AddRange(userCategories);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully set up {Count} default categories for user {UserId}",
                    userCategories.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up default categories for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Category>> GetDefaultExpenseCategoriesAsync()
        {
            return await Task.FromResult(new List<Category>
            {
                new Category { Name = "Ăn uống", Type = "EXPENSE", Description = "Chi phí ăn uống, nhà hàng, cafe" },
                new Category { Name = "Giao thông", Type = "EXPENSE", Description = "Xăng xe, taxi, xe bus, grab" },
                new Category { Name = "Mua sắm", Type = "EXPENSE", Description = "Quần áo, giày dép, đồ dùng cá nhân" },
                new Category { Name = "Giải trí", Type = "EXPENSE", Description = "Xem phim, du lịch, game, sách" },
                new Category { Name = "Sức khỏe", Type = "EXPENSE", Description = "Khám bệnh, thuốc men, gym, spa" },
                new Category { Name = "Hóa đơn", Type = "EXPENSE", Description = "Điện, nước, internet, điện thoại" },
                new Category { Name = "Giáo dục", Type = "EXPENSE", Description = "Học phí, sách vở, khóa học" },
                new Category { Name = "Nhà cửa", Type = "EXPENSE", Description = "Tiền thuê nhà, sửa chữa, đồ dùng gia đình" },
                new Category { Name = "Bảo hiểm", Type = "EXPENSE", Description = "Bảo hiểm y tế, xe, nhà" },
                new Category { Name = "Khác", Type = "EXPENSE", Description = "Các chi phí khác" }
            });
        }

        public async Task<List<Category>> GetDefaultIncomeCategoriesAsync()
        {
            return await Task.FromResult(new List<Category>
            {
                new Category { Name = "Lương", Type = "INCOME", Description = "Lương chính từ công việc" },
                new Category { Name = "Thưởng", Type = "INCOME", Description = "Thưởng hiệu suất, lễ tết" },
                new Category { Name = "Freelance", Type = "INCOME", Description = "Thu nhập từ công việc tự do" },
                new Category { Name = "Đầu tư", Type = "INCOME", Description = "Lợi nhuận từ đầu tư, cổ tức" },
                new Category { Name = "Kinh doanh", Type = "INCOME", Description = "Thu nhập từ kinh doanh" },
                new Category { Name = "Cho thuê", Type = "INCOME", Description = "Thu nhập từ cho thuê nhà, xe" },
                new Category { Name = "Quà tặng", Type = "INCOME", Description = "Tiền quà, hỗ trợ từ gia đình" },
                new Category { Name = "Khác", Type = "INCOME", Description = "Các nguồn thu nhập khác" }
            });
        }
    }
}
