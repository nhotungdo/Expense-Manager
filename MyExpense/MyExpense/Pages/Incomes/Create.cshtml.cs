using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Incomes
{
    public class CreateModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public CreateModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public CreateIncomeInput Input { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
            if (Input.IncomeDate == default)
                Input.IncomeDate = DateOnly.FromDateTime(DateTime.Today);
            if (string.IsNullOrEmpty(Input.Currency))
                Input.Currency = "VND";
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

            var entity = new Income
            {
                UserId = userId,
                CategoryId = Input.CategoryId,
                Amount = Input.Amount,
                Currency = Input.Currency,
                Note = Input.Note,
                IncomeDate = Input.IncomeDate,
                CreatedAt = DateTime.UtcNow
            };
            _db.Incomes.Add(entity);
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

        public class CreateIncomeInput
        {
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


