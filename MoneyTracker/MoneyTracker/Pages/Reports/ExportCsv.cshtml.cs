using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class ExportCsvModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExportCsvModel(ExpenseManagerContext db) { _db = db; }

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
        var items = await q.OrderByDescending(t => t.TransactionDate).Take(10000).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Date,Type,Account,Category,Amount,Currency,Note");
        foreach (var t in items)
        {
            string Esc(string s) { if (s == null) return ""; return (s.Contains(',') || s.Contains('"') || s.Contains('\n')) ? "\"" + s.Replace("\"", "\"\"") + "\"" : s; }
            sb.AppendLine(string.Join(',', new[]{
                t.TransactionDate.ToString("yyyy-MM-dd"),
                Esc(t.TransactionType == 1 ? "Income" : t.TransactionType == 0 ? "Expense" : ""),
                Esc(t.Account?.Name ?? ""),
                Esc(t.Category?.Name ?? ""),
                t.Amount.ToString("0.##"),
                Esc(t.Currency ?? "VND"),
                Esc(t.Note ?? "")
            }));
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "report.csv");
    }
}


