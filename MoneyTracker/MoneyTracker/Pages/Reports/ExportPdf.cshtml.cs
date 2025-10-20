using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class ExportPdfModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExportPdfModel(ExpenseManagerContext db) { _db = db; }

    public async Task<IActionResult> OnGetAsync(DateTime? from, DateTime? to, string? type)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var q = _db.Transactions.Include(t => t.Category).Include(t => t.Account)
            .Where(t => t.UserId == userId).AsQueryable();
        if (from.HasValue) q = q.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue) q = q.Where(t => t.TransactionDate <= to.Value);
        if (!string.IsNullOrWhiteSpace(type))
        {
            var tval = type == "Income" ? 1 : type == "Expense" ? 0 : (int?)null;
            if (tval.HasValue) q = q.Where(t => t.TransactionType == tval.Value);
        }
        var items = await q.OrderByDescending(t => t.TransactionDate).Take(1000).ToListAsync();

        // Simple HTML-to-PDF approach (for production, use a proper PDF library)
        var html = $@"
<!DOCTYPE html>
<html>
<head><title>Transaction Report</title></head>
<body>
<h2>Transaction Report</h2>
<p>Period: {from?.ToString("yyyy-MM-dd")} to {to?.ToString("yyyy-MM-dd")}</p>
<table border='1' style='border-collapse:collapse; width:100%'>
<tr><th>Date</th><th>Type</th><th>Account</th><th>Category</th><th>Amount</th><th>Note</th></tr>
{string.Join("", items.Select(t => $"<tr><td>{t.TransactionDate:yyyy-MM-dd}</td><td>{t.TransactionType}</td><td>{t.Account?.Name}</td><td>{t.Category?.Name}</td><td>{t.Amount:N2}</td><td>{t.Note}</td></tr>"))}
</table>
</body>
</html>";

        return Content(html, "text/html");
    }
}
