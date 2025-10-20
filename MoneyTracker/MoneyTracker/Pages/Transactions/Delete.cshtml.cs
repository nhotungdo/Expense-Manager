using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Transactions;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public DeleteModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public long Id { get; set; }
    public Transaction? Item { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Item = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (Item == null) return RedirectToPage("Index");
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == Id && t.UserId == userId);
        if (tx != null)
        {
            _db.Transactions.Remove(tx);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}


