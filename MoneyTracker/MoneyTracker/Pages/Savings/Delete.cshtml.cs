using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Savings;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public DeleteModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public long Id { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var g = await _db.SavingsGoals.FirstOrDefaultAsync(x => x.Id == Id && x.UserId == userId);
        if (g != null)
        {
            _db.SavingsGoals.Remove(g);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}


