using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<CategoryService> _logger;
        private readonly IAuditService _auditService;

        public CategoryService(ExpenseManagerContext context, ILogger<CategoryService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync(long userId, string? type = null)
        {
            var query = _context.Categories.Where(c => c.UserId == userId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(c => c.Type == type);

            return await query
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(long id, long userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Category> CreateCategoryAsync(CategoryDto categoryDto, long userId)
        {
            var category = new Category
            {
                UserId = userId,
                Name = categoryDto.Name,
                Type = categoryDto.Type,
                Description = categoryDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "CREATE", $"Created category: {category.Name} ({category.Type})", "Category", category.Id);

            _logger.LogInformation("Category created for user {UserId}: {CategoryId}", userId, category.Id);
            return category;
        }

        public async Task<Category?> UpdateCategoryAsync(long id, CategoryDto categoryDto, long userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return null;

            var oldName = category.Name;
            var oldType = category.Type;

            category.Name = categoryDto.Name;
            category.Type = categoryDto.Type;
            category.Description = categoryDto.Description;

            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "UPDATE",
                $"Updated category: {oldName} ({oldType}) -> {category.Name} ({category.Type})",
                "Category", category.Id);

            _logger.LogInformation("Category updated for user {UserId}: {CategoryId}", userId, category.Id);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(long id, long userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return false;

            // Check if category is being used
            var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == id);
            var hasIncomes = await _context.Incomes.AnyAsync(i => i.CategoryId == id);

            if (hasExpenses || hasIncomes)
            {
                _logger.LogWarning("Cannot delete category {CategoryId} as it is being used", id);
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "DELETE", $"Deleted category: {category.Name}", "Category", category.Id);

            _logger.LogInformation("Category deleted for user {UserId}: {CategoryId}", userId, category.Id);
            return true;
        }

        public async Task<IEnumerable<Category>> GetDefaultCategoriesAsync()
        {
            return new List<Category>
            {
                new Category { Name = "Ăn uống", Type = "EXPENSE", Description = "Chi phí ăn uống hàng ngày" },
                new Category { Name = "Giao thông", Type = "EXPENSE", Description = "Chi phí đi lại, xăng xe" },
                new Category { Name = "Mua sắm", Type = "EXPENSE", Description = "Mua sắm quần áo, đồ dùng" },
                new Category { Name = "Giải trí", Type = "EXPENSE", Description = "Chi phí giải trí, du lịch" },
                new Category { Name = "Y tế", Type = "EXPENSE", Description = "Chi phí khám chữa bệnh" },
                new Category { Name = "Học tập", Type = "EXPENSE", Description = "Chi phí học tập, sách vở" },
                new Category { Name = "Lương", Type = "INCOME", Description = "Thu nhập từ lương" },
                new Category { Name = "Thưởng", Type = "INCOME", Description = "Thu nhập từ thưởng" },
                new Category { Name = "Đầu tư", Type = "INCOME", Description = "Thu nhập từ đầu tư" },
                new Category { Name = "Kinh doanh", Type = "INCOME", Description = "Thu nhập từ kinh doanh" }
            };
        }

        public async Task<bool> InitializeDefaultCategoriesAsync(long userId)
        {
            try
            {
                var existingCategories = await _context.Categories
                    .Where(c => c.UserId == userId)
                    .CountAsync();

                if (existingCategories > 0)
                    return true; // Already initialized

                var defaultCategories = await GetDefaultCategoriesAsync();
                foreach (var category in defaultCategories)
                {
                    category.UserId = userId;
                    category.CreatedAt = DateTime.UtcNow;
                }

                _context.Categories.AddRange(defaultCategories);
                await _context.SaveChangesAsync();

                await _auditService.LogUserActionAsync(userId, "INITIALIZE", "Initialized default categories", "Category");

                _logger.LogInformation("Default categories initialized for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize default categories for user {UserId}", userId);
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetCategoryUsageStatsAsync(long userId)
        {
            var expenseStats = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var incomeStats = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .GroupBy(i => i.Category != null ? i.Category.Name : "Uncategorized")
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var combinedStats = new Dictionary<string, int>();

            foreach (var stat in expenseStats.Concat(incomeStats))
            {
                if (combinedStats.ContainsKey(stat.Key))
                    combinedStats[stat.Key] += stat.Value;
                else
                    combinedStats[stat.Key] = stat.Value;
            }

            return combinedStats;
        }
    }
}
