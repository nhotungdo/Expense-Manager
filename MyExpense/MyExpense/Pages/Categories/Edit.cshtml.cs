using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public EditModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        [BindProperty]
        public EditCategoryInput Input { get; set; } = new();
        [BindProperty]
        public bool IsShared { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);

            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return RedirectToPage("/Categories/Index");

            // only allow edit if shared or owned by user
            if (c.UserId != null && c.UserId != userId)
                return RedirectToPage("/Categories/Index");

            Input = new EditCategoryInput { Id = c.Id, Name = c.Name, Type = c.Type };
            IsShared = c.UserId == null;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userIdStr = User.FindFirst("app:userId")?.Value;
            long.TryParse(userIdStr, out var userId);

            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == Input.Id);
            if (c == null) return RedirectToPage("/Categories/Index");

            if (c.UserId != null && c.UserId != userId)
                return RedirectToPage("/Categories/Index");

            c.Name = Input.Name;
            c.Type = Input.Type;
            c.UserId = IsShared ? null : userId;
            await _db.SaveChangesAsync();
            return RedirectToPage("/Categories/Index");
        }

        public class EditCategoryInput
        {
            public long Id { get; set; }
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            [RegularExpression("^(EXPENSE|INCOME)$")]
            public string Type { get; set; } = "EXPENSE";
        }
    }
}


