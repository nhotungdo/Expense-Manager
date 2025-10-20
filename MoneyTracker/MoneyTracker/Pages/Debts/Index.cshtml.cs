using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Debts;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }
    public List<Debt> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Items = await _db.Debts.Where(d => d.UserId == userId).OrderBy(d => d.Status).ThenBy(d => d.DueDate).ToListAsync();
    }
}


