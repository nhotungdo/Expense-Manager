using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class BudgetVsActualModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public BudgetVsActualModel(ExpenseManagerContext db) { _db = db; }

    public List<Row> Rows { get; set; } = new();
    public class Row { public string CategoryName { get; set; } = string.Empty; public decimal Budget { get; set; } public decimal Spent { get; set; } }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = start.AddMonths(1).AddTicks(-1);
        var budgets = await _db.Budgets.Where(b => b.UserId == userId && b.Period == 1 && b.StartDate == start && b.EndDate == end).ToListAsync();
        var names = await _db.Categories.ToDictionaryAsync(c => c.Id, c => c.Name);
        var spend = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        Rows = budgets.Select(b => new Row
        {
            CategoryName = b.CategoryId.HasValue && names.ContainsKey(b.CategoryId.Value) ? names[b.CategoryId.Value] : "(All)",
            Budget = b.Amount,
            Spent = spend.FirstOrDefault(x => x.CategoryId == b.CategoryId)?.Total ?? 0m
        }).OrderByDescending(r => r.Spent).ToList();
    }
}


