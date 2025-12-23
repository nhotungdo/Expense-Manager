using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTrackerApp.Pages.Reports
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public long? AccountId { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetIncome { get; set; }
        public decimal SavingsRate { get; set; }

        public List<string> ExpenseCategoryLabels { get; set; } = new();
        public List<decimal> ExpenseCategoryData { get; set; } = new();
        public List<string> ExpenseCategoryColors { get; set; } = new();

        public List<string> DailyLabels { get; set; } = new();
        public List<decimal> DailyIncomeData { get; set; } = new();
        public List<decimal> DailyExpenseData { get; set; } = new();

        public List<MoneyTrackerApp.Models.Account> Accounts { get; set; } = new();
        public List<MoneyTrackerApp.Models.Transaction> Transactions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Default to this month
            var now = DateTime.Now;
            if (!StartDate.HasValue) StartDate = new DateTime(now.Year, now.Month, 1);
            if (!EndDate.HasValue) EndDate = now.Date;

            // Load Accounts for filter
            Accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Query Transactions
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

            if (AccountId.HasValue)
            {
                query = query.Where(t => t.AccountId == AccountId.Value);
            }

            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
            Transactions = transactions;

            // Calculate Totals
            TotalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount); // 1 = Income
            TotalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount); // 2 = Expense
            NetIncome = TotalIncome - TotalExpense;
            SavingsRate = TotalIncome > 0 ? ((TotalIncome - TotalExpense) / TotalIncome) * 100 : 0;

            // Prepare Pie Chart (Expense by Category)
            var expensesByCategory = transactions
                .Where(t => t.TransactionType == 2 && t.Category != null)
                .GroupBy(t => t.Category!.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Color = g.First().Category!.Color // Assuming Category has Color
                })
                .OrderByDescending(x => x.Amount)
                .Take(10) // Top 10 categories
                .ToList();

            ExpenseCategoryLabels = expensesByCategory.Select(x => x.Category).ToList();
            ExpenseCategoryData = expensesByCategory.Select(x => x.Amount).ToList();
            ExpenseCategoryColors = expensesByCategory.Select(x => x.Color ?? "#cbd5e1").ToList(); // Default color if null

            // Prepare Line Chart (Daily Trend)
            var dailyData = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Income = g.Where(t => t.TransactionType == 1).Sum(t => t.Amount),
                    Expense = g.Where(t => t.TransactionType == 2).Sum(t => t.Amount)
                })
                .OrderBy(x => x.Date)
                .ToList();
            
            // Fill in missing dates if range is small enough, e.g. < 60 days
            var days = (EndDate.Value - StartDate.Value).Days;
            if (days < 60)
            {
                for (var d = StartDate.Value; d <= EndDate.Value; d = d.AddDays(1))
                {
                    if (!dailyData.Any(x => x.Date == d))
                    {
                        dailyData.Add(new { Date = d, Income = 0m, Expense = 0m });
                    }
                }
                dailyData = dailyData.OrderBy(x => x.Date).ToList();
            }

            DailyLabels = dailyData.Select(x => x.Date.ToString("dd/MM")).ToList();
            DailyIncomeData = dailyData.Select(x => x.Income).ToList();
            DailyExpenseData = dailyData.Select(x => x.Expense).ToList();

            return Page();
        }
    }
}
