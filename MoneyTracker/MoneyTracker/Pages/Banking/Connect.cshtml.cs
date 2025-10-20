using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Banking;

[Authorize]
public class ConnectModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ConnectModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<MoneyTracker.Models.Account> Accounts { get; set; } = new();

    public class InputModel
    {
        public string Provider { get; set; } = "";
        public long? AccountId { get; set; }
        public string Credentials { get; set; } = "";
    }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        Accounts = await _db.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Create bank connection (demo implementation)
        var connection = new BankConnection
        {
            UserId = userId,
            Provider = Input.Provider,
            AccountId = Input.AccountId.GetValueOrDefault(),
            SyncStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.BankConnections.Add(connection);
        await _db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
