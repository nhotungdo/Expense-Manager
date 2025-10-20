using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reports;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public List<Transaction> Items { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterModel Filter { get; set; } = new();

    public class FilterModel
    {
        [DataType(DataType.Date)] public DateTime? From { get; set; }
        [DataType(DataType.Date)] public DateTime? To { get; set; }
        public string? Type { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var q = _db.Transactions.Include(t => t.Category).Include(t => t.Account)
            .Where(t => t.UserId == userId)
            .AsQueryable();
        if (Filter.From.HasValue) q = q.Where(t => t.TransactionDate >= Filter.From.Value);
        if (Filter.To.HasValue) q = q.Where(t => t.TransactionDate <= Filter.To.Value);
        if (!string.IsNullOrWhiteSpace(Filter.Type))
        {
            var type = Filter.Type == "Income" ? 1 : Filter.Type == "Expense" ? 0 : (int?)null;
            if (type.HasValue) q = q.Where(t => t.TransactionType == type.Value);
        }
        Items = await q.OrderByDescending(t => t.TransactionDate).Take(2000).ToListAsync();
    }

    public async Task<IActionResult> OnGetChartData()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var start = Filter.From ?? DateTime.Today.AddDays(-30);
        var end = Filter.To ?? DateTime.Today;

        // Category spending
        var categoryData = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var categoryNames = await _db.Categories.ToDictionaryAsync(c => c.Id, c => c.Name);
        var categories = categoryData.Select(x => x.CategoryId.HasValue && categoryNames.ContainsKey(x.CategoryId.Value) ? categoryNames[x.CategoryId.Value] : "Uncategorized").ToList();
        var categoryAmounts = categoryData.Select(x => x.Total).ToList();

        // Daily trend
        var days = Enumerable.Range(0, (end - start).Days + 1).Select(i => start.AddDays(i)).ToList();
        var dailyData = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var dailyMap = dailyData.ToDictionary(x => x.Date, x => x.Total);
        var dailyTotals = days.Select(d => dailyMap.ContainsKey(d) ? dailyMap[d] : 0m).ToList();

        return new JsonResult(new
        {
            categories,
            categoryAmounts,
            days = days.Select(d => d.ToString("MM-dd")).ToList(),
            dailyTotals
        });
    }
}


