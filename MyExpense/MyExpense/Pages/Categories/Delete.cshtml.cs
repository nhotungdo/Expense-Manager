using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public DeleteModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public Category? Item { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);

            Item = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (Item == null) return RedirectToPage("/Categories/Index");
            if (Item.UserId != null && Item.UserId != userId)
                return RedirectToPage("/Categories/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);

            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c != null && (c.UserId == null || c.UserId == userId))
            {
                _db.Categories.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Categories/Index");
        }
    }
}


