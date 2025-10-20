using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Banking;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public List<BankConnection> Connections { get; set; } = new();
    public string? SyncResult { get; set; }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        Connections = await _db.BankConnections
            .Include(b => b.Account)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetSyncAsync(int id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        var conn = await _db.BankConnections
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (conn == null) return NotFound();

        // Simulate bank sync (in production, integrate with real banking APIs)
        try
        {
            // Update sync status
            conn.SyncStatus = "Success";
            conn.CreatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Simulate importing transactions
            var mockTransactions = new[]
            {
                new Transaction
                {
                    UserId = userId,
                    AccountId = conn.AccountId,
                    TransactionType = 0,
                    Amount = 25.50m,
                    TransactionDate = DateTime.Today.AddDays(-1),
                    Note = "Coffee Shop - Auto-imported from bank"
                },
                new Transaction
                {
                    UserId = userId,
                    AccountId = conn.AccountId,
                    TransactionType = 1,
                    Amount = 1200.00m,
                    TransactionDate = DateTime.Today.AddDays(-2),
                    Note = "Salary - Auto-imported from bank"
                }
            };

            _db.Transactions.AddRange(mockTransactions);
            await _db.SaveChangesAsync();

            SyncResult = $"Successfully synced {mockTransactions.Length} transactions from {conn.Provider}";
        }
        catch (Exception ex)
        {
            conn.SyncStatus = "Failed";
            await _db.SaveChangesAsync();
            SyncResult = $"Sync failed: {ex.Message}";
        }

        await OnGetAsync();
        return Page();
    }
}
