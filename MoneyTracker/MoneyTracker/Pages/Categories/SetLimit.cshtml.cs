using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Categories;

[Authorize]
public class SetLimitModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public SetLimitModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string CategoryName { get; set; } = string.Empty;

    public class InputModel
    {
        public long Id { get; set; }
        [Range(typeof(decimal), "0.00", "9999999999")]
        public decimal Amount { get; set; }
        [Required]
        public string Month { get; set; } = DateTime.Now.ToString("yyyy-MM");
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id && (x.UserId == userId || x.IsDefault));
        if (c == null) return RedirectToPage("Index");
        CategoryName = c.Name;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == Input.Id && (x.UserId == userId || x.IsDefault));
        if (c == null) return RedirectToPage("Index");

        var parts = Input.Month.Split('-');
        var year = int.Parse(parts[0]);
        var month = int.Parse(parts[1]);
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddTicks(-1);

        var existing = await _db.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == c.Id && b.Period == 1 && b.StartDate == start && b.EndDate == end);
        if (existing == null)
        {
            existing = new Budget
            {
                UserId = userId,
                CategoryId = c.Id,
                Period = 1, // monthly
                StartDate = start,
                EndDate = end,
                Amount = Input.Amount,
                CreatedAt = DateTime.UtcNow
            };
            _db.Budgets.Add(existing);
        }
        else
        {
            existing.Amount = Input.Amount;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


