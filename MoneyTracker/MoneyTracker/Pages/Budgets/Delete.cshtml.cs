using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Budgets;

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
        var b = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == Id && x.UserId == userId);
        if (b != null)
        {
            _db.Budgets.Remove(b);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}


