using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public decimal ThisMonthIncome { get; set; }
    public decimal ThisMonthExpense { get; set; }
    public decimal Net => ThisMonthIncome - ThisMonthExpense;
    public int TxCount { get; set; }

    public List<SpendingRow> SpendingByCategory { get; set; } = new();

    public class SpendingRow
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public async Task OnGet()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = start.AddMonths(1).AddTicks(-1);

        ThisMonthIncome = await _db.Transactions.Where(t => t.UserId == userId && t.TransactionType == 1 && t.TransactionDate >= start && t.TransactionDate <= end)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        ThisMonthExpense = await _db.Transactions.Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        TxCount = await _db.Transactions.CountAsync(t => t.UserId == userId && t.TransactionDate >= start && t.TransactionDate <= end);

        var byCat = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var names = await _db.Categories.ToDictionaryAsync(c => c.Id, c => c.Name);
        SpendingByCategory = byCat.Select(x => new SpendingRow
        {
            CategoryName = x.CategoryId.HasValue && names.ContainsKey(x.CategoryId.Value) ? names[x.CategoryId.Value] : "(Uncategorized)",
            Total = x.Total
        }).OrderByDescending(r => r.Total).ToList();
    }

    public async Task<IActionResult> OnGetChartData()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = start.AddMonths(1).AddTicks(-1);

        var days = Enumerable.Range(0, DateTime.DaysInMonth(start.Year, start.Month))
            .Select(i => start.AddDays(i).ToString("dd"))
            .ToList();
        var map = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new { Day = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var dict = map.ToDictionary(x => x.Day.ToString("dd"), x => x.Total);
        var dailyExpense = days.Select(d => dict.ContainsKey(d) ? dict[d] : 0m).ToList();

        var income = await _db.Transactions.Where(t => t.UserId == userId && t.TransactionType == 1 && t.TransactionDate >= start && t.TransactionDate <= end)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        var expense = await _db.Transactions.Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        return new JsonResult(new { days, dailyExpense, income, expense });
    }
}


