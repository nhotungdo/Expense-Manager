using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Budgets;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public List<Budget> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Items = await _db.Budgets.Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartDate)
            .Take(200).ToListAsync();
    }
}


