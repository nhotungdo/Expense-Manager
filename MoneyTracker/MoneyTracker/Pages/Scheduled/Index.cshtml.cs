using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Scheduled;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }
    public List<ScheduledTransaction> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Items = await _db.ScheduledTransactions.Include(s => s.Account)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.NextRunDate)
            .ToListAsync();
    }
}


