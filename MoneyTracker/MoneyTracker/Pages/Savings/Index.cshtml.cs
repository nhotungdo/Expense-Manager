using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Savings;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }
    public List<SavingsGoal> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Items = await _db.SavingsGoals.Where(g => g.UserId == userId)
            .OrderBy(g => g.Status).ThenBy(g => g.Name).ToListAsync();
    }
}


