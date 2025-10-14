using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetSystemCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.UserId == null && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Categories.CountAsync();
    }
}
