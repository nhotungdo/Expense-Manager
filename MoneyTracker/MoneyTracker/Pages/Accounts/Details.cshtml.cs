using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Accounts;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public DetailsModel(ExpenseManagerContext db) { _db = db; }

    public MoneyTracker.Models.Account? Account { get; set; }
    public List<Transaction> Transactions { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        Account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        Transactions = await _db.Transactions.Include(t => t.Category)
            .Where(t => t.UserId == userId && t.AccountId == id)
            .OrderByDescending(t => t.TransactionDate)
            .Take(200).ToListAsync();
    }
}


