using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Expenses
{
    public class EditModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public EditModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public EditExpenseInput Input { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            var e = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (e == null) return RedirectToPage("/Expenses/Index");

            Input = new EditExpenseInput
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                CategoryId = e.CategoryId,
                Amount = e.Amount,
                Currency = e.Currency ?? "VND",
                Note = e.Note
            };
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var e = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (e == null) return RedirectToPage("/Expenses/Index");

            e.ExpenseDate = Input.ExpenseDate;
            e.CategoryId = Input.CategoryId;
            e.Amount = Input.Amount;
            e.Currency = Input.Currency;
            e.Note = Input.Note;
            await _db.SaveChangesAsync();

            return RedirectToPage("/Expenses/Index");
        }

        private async Task LoadCategoriesAsync()
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);
            var cats = await _db.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Type).ThenBy(c => c.Name)
                .ToListAsync();
            CategoryOptions = cats.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name} ({c.Type})"
            });
        }

        public class EditExpenseInput
        {
            public long Id { get; set; }
            [Required]
            public DateOnly ExpenseDate { get; set; }
            public long? CategoryId { get; set; }
            [Range(0.01, double.MaxValue)]
            public decimal Amount { get; set; }
            [Required]
            public string Currency { get; set; } = "VND";
            public string? Note { get; set; }
        }
    }
}


