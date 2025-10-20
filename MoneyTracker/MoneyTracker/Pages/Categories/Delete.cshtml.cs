using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Categories;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public DeleteModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty]
    public long Id { get; set; }
    public Category? Item { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Item = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && (c.UserId == userId || c.IsDefault));
        if (Item == null) return RedirectToPage("Index");
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == Id && (x.UserId == userId || x.IsDefault));
        if (c != null)
        {
            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}


