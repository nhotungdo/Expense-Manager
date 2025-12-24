using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Pages.Admin.MasterData.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        [BindProperty]
        public Category CreateCategory { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch System Categories (IsDefault = true or UserId = null)
            Categories = await _context.Categories
                .Where(c => c.IsDefault || c.UserId == null)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Manual creation for System Categories
            var category = new Category
            {
                Name = CreateCategory.Name,
                Type = CreateCategory.Type,
                Icon = CreateCategory.Icon,
                Color = CreateCategory.Color,
                Description = CreateCategory.Description,
                IsDefault = true, 
                UserId = null, // System Category
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Basic check, might want to check usage before delete
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
