using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyExpense.Models;

namespace MyExpense.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public CreateModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public CreateCategoryInput Input { get; set; } = new();
        [BindProperty]
        public bool IsShared { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            long? userId = null;
            if (!IsShared)
            {
                var userIdStr = User.FindFirst("app:userId")?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Account/Login");
                userId = long.Parse(userIdStr);
            }

            var entity = new Category
            {
                Name = Input.Name,
                Type = Input.Type,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToPage("/Categories/Index");
        }

        public class CreateCategoryInput
        {
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            [RegularExpression("^(EXPENSE|INCOME)$")]
            public string Type { get; set; } = "EXPENSE";
        }
    }
}


