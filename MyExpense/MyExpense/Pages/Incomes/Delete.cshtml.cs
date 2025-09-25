using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Incomes
{
    public class DeleteModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public DeleteModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public Income? Item { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            Item = await _db.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (Item == null) return RedirectToPage("/Incomes/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            var i = await _db.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (i != null)
            {
                _db.Incomes.Remove(i);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Incomes/Index");
        }
    }
}


