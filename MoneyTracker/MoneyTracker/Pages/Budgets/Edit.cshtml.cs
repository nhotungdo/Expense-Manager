using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Budgets;

[Authorize]
public class EditModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public EditModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        [Range(1,3)] public int Period { get; set; } = 1;
        [DataType(DataType.Date)] public DateTime StartDate { get; set; } = DateTime.Today;
        [DataType(DataType.Date)] public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1).AddDays(-1);
        [Range(typeof(decimal), "0.00", "9999999999")] public decimal Amount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long? id)
    {
        await LoadOptionsAsync();
        if (id.HasValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            long.TryParse(userIdStr, out var userId);
            var b = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (b == null) return RedirectToPage("Index");
            Input = new InputModel
            {
                Id = b.Id,
                CategoryId = b.CategoryId,
                Period = b.Period,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Amount = b.Amount
            };
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        if (Input.Id == 0)
        {
            var b = new Budget
            {
                UserId = userId,
                CategoryId = Input.CategoryId,
                Period = Input.Period,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                Amount = Input.Amount,
                CreatedAt = DateTime.UtcNow
            };
            _db.Budgets.Add(b);
        }
        else
        {
            var b = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
            if (b == null) return RedirectToPage("Index");
            b.CategoryId = Input.CategoryId;
            b.Period = Input.Period;
            b.StartDate = Input.StartDate;
            b.EndDate = Input.EndDate;
            b.Amount = Input.Amount;
            b.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task LoadOptionsAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        CategoryOptions = await _db.Categories
            .Where(c => (c.UserId == userId || c.IsDefault) && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }
}


