using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Debts;

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
        var d = await _db.Debts.FirstOrDefaultAsync(x => x.Id == Id && x.UserId == userId);
        if (d != null)
        {
            _db.Debts.Remove(d);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}


