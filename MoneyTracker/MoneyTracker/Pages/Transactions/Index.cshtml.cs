using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Transactions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public List<Transaction> Items { get; set; } = new();
    public List<LimitAlert> Alerts { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterModel Filter { get; set; } = new();

    public class FilterModel
    {
        public string? Query { get; set; }
        [DataType(DataType.Date)] public DateTime? From { get; set; }
        [DataType(DataType.Date)] public DateTime? To { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var q = _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Filter.Query))
        {
            var s = Filter.Query.Trim();
            q = q.Where(t => t.Note != null && t.Note.Contains(s));
        }
        if (Filter.From.HasValue) q = q.Where(t => t.TransactionDate >= Filter.From.Value);
        if (Filter.To.HasValue) q = q.Where(t => t.TransactionDate <= Filter.To.Value);

        Items = await q.OrderByDescending(t => t.TransactionDate).Take(200).ToListAsync();

        // compute budget alerts for current month
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = start.AddMonths(1).AddTicks(-1);
        var budgets = await _db.Budgets.Where(b => b.UserId == userId && b.Period == 1 && b.StartDate == start && b.EndDate == end).ToListAsync();
        var byCat = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 && t.TransactionDate >= start && t.TransactionDate <= end)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();
        var categories = await _db.Categories.ToDictionaryAsync(c => c.Id, c => c.Name);
        foreach (var b in budgets)
        {
            var spent = byCat.FirstOrDefault(x => x.CategoryId == b.CategoryId)?.Total ?? 0m;
            if (b.Amount <= 0) continue;
            var ratio = spent / b.Amount;
            if (ratio >= 0.8m)
            {
                Alerts.Add(new LimitAlert
                {
                    CategoryName = (b.CategoryId.HasValue && categories.ContainsKey(b.CategoryId.Value)) ? categories[b.CategoryId.Value] : "(All)",
                    Spent = spent,
                    Limit = b.Amount
                });
            }
        }
    }

    public class LimitAlert
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Spent { get; set; }
        public decimal Limit { get; set; }
    }
}


