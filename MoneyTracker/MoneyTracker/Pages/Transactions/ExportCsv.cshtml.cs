using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Transactions;

[Authorize]
public class ExportCsvModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExportCsvModel(ExpenseManagerContext db) { _db = db; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var items = await _db.Transactions.Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(1000)
            .ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Date,Type,AccountId,Category,Amount,Currency,Note");
        foreach (var t in items)
        {
            var line = string.Join(',', new[]
            {
                t.TransactionDate.ToString("yyyy-MM-dd"),
                Escape(t.TransactionType == 1 ? "Income" : t.TransactionType == 0 ? "Expense" : ""),
                t.AccountId.ToString(),
                Escape(t.Category?.Name ?? ""),
                t.Amount.ToString("0.##"),
                Escape(t.Currency ?? "VND"),
                Escape(t.Note ?? "")
            });
            sb.AppendLine(line);
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "transactions.csv");
    }

    private static string Escape(string s)
    {
        if (s == null) return string.Empty;
        if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
        {
            s = s.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }
        return s;
    }
}


