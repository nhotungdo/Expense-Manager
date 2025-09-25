using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Incomes
{
    public class EditModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public EditModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public EditIncomeInput Input { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
            var userId = long.Parse(userIdStr);

            var i = await _db.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (i == null) return RedirectToPage("/Incomes/Index");

            Input = new EditIncomeInput
            {
                Id = i.Id,
                IncomeDate = i.IncomeDate,
                CategoryId = i.CategoryId,
                Amount = i.Amount,
                Currency = i.Currency ?? "VND",
                Note = i.Note
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

            var i = await _db.Incomes.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (i == null) return RedirectToPage("/Incomes/Index");

            i.IncomeDate = Input.IncomeDate;
            i.CategoryId = Input.CategoryId;
            i.Amount = Input.Amount;
            i.Currency = Input.Currency;
            i.Note = Input.Note;
            await _db.SaveChangesAsync();

            return RedirectToPage("/Incomes/Index");
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

        public class EditIncomeInput
        {
            public long Id { get; set; }
            [Required]
            public DateOnly IncomeDate { get; set; }
            public long? CategoryId { get; set; }
            [Range(0.01, double.MaxValue)]
            public decimal Amount { get; set; }
            [Required]
            public string Currency { get; set; } = "VND";
            public string? Note { get; set; }
        }
    }
}


