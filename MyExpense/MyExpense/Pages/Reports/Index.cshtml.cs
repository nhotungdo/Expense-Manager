using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyExpense.Models;

namespace MyExpense.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public string? FromStr => From?.ToString("yyyy-MM-dd");
        public string? ToStr => To?.ToString("yyyy-MM-dd");
        public string Currency { get; set; } = "VND";

        public decimal TotalExpense { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;

        public string GroupLabel { get; set; } = "ngày";
        public IList<Row> Rows { get; set; } = new List<Row>();

        public async Task OnGetAsync(DateOnly? from, DateOnly? to, string group = "day")
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return;
            var userId = long.Parse(userIdStr);

            From = from ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            To = to ?? DateOnly.FromDateTime(DateTime.Today);

            var eQuery = _db.Expenses.Where(e => e.UserId == userId && e.ExpenseDate >= From && e.ExpenseDate <= To);
            var iQuery = _db.Incomes.Where(i => i.UserId == userId && i.IncomeDate >= From && i.IncomeDate <= To);

            TotalExpense = await eQuery.SumAsync(e => (decimal?)e.Amount) ?? 0;
            TotalIncome = await iQuery.SumAsync(i => (decimal?)i.Amount) ?? 0;
            Currency = (await _db.Expenses.Where(e => e.UserId == userId).Select(e => e.Currency).FirstOrDefaultAsync()) ?? "VND";

            switch (group)
            {
                case "month":
                    GroupLabel = "tháng";
                    Rows = await BuildRowsAsync(e => new { Label = new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, 1) },
                                                  i => new { Label = new DateTime(i.IncomeDate.Year, i.IncomeDate.Month, 1) });
                    break;
                case "year":
                    GroupLabel = "năm";
                    Rows = await BuildRowsAsync(e => new { Label = new DateTime(e.ExpenseDate.Year, 1, 1) },
                                                  i => new { Label = new DateTime(i.IncomeDate.Year, 1, 1) });
                    break;
                default:
                    GroupLabel = "ngày";
                    Rows = await BuildRowsAsync(e => new { Label = new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, e.ExpenseDate.Day) },
                                                  i => new { Label = new DateTime(i.IncomeDate.Year, i.IncomeDate.Month, i.IncomeDate.Day) });
                    break;
            }
        }

        private async Task<IList<Row>> BuildRowsAsync(
            System.Linq.Expressions.Expression<Func<Expense, object>> expenseKey,
            System.Linq.Expressions.Expression<Func<Income, object>> incomeKey)
        {
            var userIdStr = User.FindFirst("app:userId")?.Value;
            var userId = long.Parse(userIdStr!);

            var eGroups = await _db.Expenses.Where(e => e.UserId == userId && e.ExpenseDate >= From && e.ExpenseDate <= To)
                .GroupBy(expenseKey)
                .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
                .ToListAsync();

            var iGroups = await _db.Incomes.Where(i => i.UserId == userId && i.IncomeDate >= From && i.IncomeDate <= To)
                .GroupBy(incomeKey)
                .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
                .ToListAsync();

            var dict = new Dictionary<string, Row>();

            foreach (var eg in eGroups)
            {
                var label = eg.Key!.ToString()!;
                if (!dict.TryGetValue(label, out var row))
                {
                    row = new Row { Label = label };
                    dict[label] = row;
                }
                row.Expense = eg.Sum;
            }

            foreach (var ig in iGroups)
            {
                var label = ig.Key!.ToString()!;
                if (!dict.TryGetValue(label, out var row))
                {
                    row = new Row { Label = label };
                    dict[label] = row;
                }
                row.Income = ig.Sum;
            }

            return dict.Values.OrderBy(x => x.Label).ToList();
        }

        public class Row
        {
            public string Label { get; set; } = string.Empty;
            public decimal Expense { get; set; }
            public decimal Income { get; set; }
        }
    }
}


