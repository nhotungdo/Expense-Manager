using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Expenses
{
    public class DeleteModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public DeleteModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public Expense? Item { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            Item = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (Item == null) return RedirectToPage("/Expenses/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            var e = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (e != null)
            {
                _db.Expenses.Remove(e);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Expenses/Index");
        }
    }
}


