using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public IList<Category> Items { get; set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);
            Items = await _db.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Type).ThenBy(c => c.Name)
                .ToListAsync();
        }
    }
}


