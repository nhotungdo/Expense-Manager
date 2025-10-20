using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class MonthYearModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public MonthYearModel(ExpenseManagerContext db) { _db = db; }

    public void OnGet() { }

    public async Task<IActionResult> OnGetData()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        var now = DateTime.Today;
        var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
        var months = Enumerable.Range(0, 12).Select(i => startMonth.AddMonths(i)).ToList();
        var monthlyAgg = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startMonth)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month, t.TransactionType })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.TransactionType, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var monthlyExpense = months.Select(m => monthlyAgg.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month && x.TransactionType == 0)?.Total ?? 0m).ToList();

        var years = Enumerable.Range(now.Year - 4, 5).ToList();
        var startYear = new DateTime(years.First(), 1, 1);
        var yearlyAgg = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= startYear)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionType })
            .Select(g => new { g.Key.Year, g.Key.TransactionType, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var yearlyIncome = years.Select(y => yearlyAgg.FirstOrDefault(x => x.Year == y && x.TransactionType == 1)?.Total ?? 0m).ToList();
        var yearlyExpense = years.Select(y => yearlyAgg.FirstOrDefault(x => x.Year == y && x.TransactionType == 0)?.Total ?? 0m).ToList();

        return new JsonResult(new
        {
            months = months.Select(m => m.ToString("yyyy-MM")).ToList(),
            monthlyExpense,
            years,
            yearlyIncome,
            yearlyExpense
        });
    }
}


