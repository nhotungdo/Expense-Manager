using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Savings;

[Authorize]
public class ContributeModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ContributeModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string GoalName { get; set; } = string.Empty;

    public class InputModel
    {
        public long Id { get; set; }
        [Range(typeof(decimal), "0.01", "9999999999")] public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var g = await _db.SavingsGoals.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (g == null) return RedirectToPage("Index");
        GoalName = g.Name;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage("/Account/Login");
        var g = await _db.SavingsGoals.FirstOrDefaultAsync(x => x.Id == Input.Id && x.UserId == userId);
        if (g == null) return RedirectToPage("Index");

        g.CurrentAmount += Input.Amount;
        g.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


