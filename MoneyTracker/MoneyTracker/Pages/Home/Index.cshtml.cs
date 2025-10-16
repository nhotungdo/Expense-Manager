using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Home
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;

        public string GreetingName { get; private set; } = "";
        public decimal TotalIncomeThisMonth { get; private set; }
        public decimal TotalExpenseThisMonth { get; private set; }
        public decimal BalanceThisMonth => TotalIncomeThisMonth - TotalExpenseThisMonth;

        public List<CategorySpending> SpendingByCategoryThisMonth { get; private set; } = new();
        public List<MonthlyTrendPoint> MonthlyIncomeExpenseTrend { get; private set; } = new();
        public List<RecentTransactionItem> RecentTransactions { get; private set; } = new();
        public List<BudgetProgressItem> BudgetProgress { get; private set; } = new();

        public IndexModel(ExpenseManagerContext db)
        {
            _db = db;
        }

        public async Task OnGet()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthStartDateOnly = DateOnly.FromDateTime(monthStart);
            var nowDateOnly = DateOnly.FromDateTime(now);
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                GreetingName = "Guest";
                return;
            }

            var googleId = User.FindFirst("sub")?.Value
                           ?? User.FindFirst("urn:google:userid")?.Value
                           ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var currentUser = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (currentUser == null)
            {
                GreetingName = "User";
                return;
            }

            GreetingName = currentUser.FullName ?? currentUser.UserName ?? "User";

            TotalIncomeThisMonth = await _db.Incomes
                .Where(i => i.UserId == currentUser.Id && i.IncomeDate >= monthStartDateOnly && i.IncomeDate <= nowDateOnly)
                .SumAsync(i => (decimal?)i.Amount) ?? 0m;

            TotalExpenseThisMonth = await _db.Expenses
                .Where(e => e.UserId == currentUser.Id && e.ExpenseDate >= monthStartDateOnly && e.ExpenseDate <= nowDateOnly)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            SpendingByCategoryThisMonth = await _db.Transactions
                .Where(t => t.UserId == currentUser.Id && t.TransactionDate >= monthStart && t.TransactionDate <= now && t.Type == 2)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Category.Icon, t.Category.Color })
                .Select(g => new CategorySpending
                {
                    CategoryId = g.Key.CategoryId ?? 0,
                    CategoryName = g.Key.Name,
                    Icon = g.Key.Icon,
                    Color = g.Key.Color,
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();

            var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var trendData = await _db.Transactions
                .Where(t => t.UserId == currentUser.Id && t.TransactionDate >= sixMonthsAgo && t.TransactionDate <= now)
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(x => x.Type == 1).Sum(x => (decimal?)x.Amount) ?? 0m,
                    Expense = g.Where(x => x.Type == 2).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToListAsync();

            var months = Enumerable.Range(0, 6)
                .Select(offset => sixMonthsAgo.AddMonths(offset))
                .Select(d => new { d.Year, d.Month })
                .ToList();

            MonthlyIncomeExpenseTrend = months
                .Select(m =>
                {
                    var item = trendData.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                    var label = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy");
                    return new MonthlyTrendPoint
                    {
                        Label = label,
                        Income = item?.Income ?? 0m,
                        Expense = item?.Expense ?? 0m
                    };
                })
                .ToList();

            RecentTransactions = await _db.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(7)
                .Select(t => new RecentTransactionItem
                {
                    Date = t.TransactionDate,
                    Amount = t.Amount,
                    Type = t.Type,
                    CategoryName = t.Category != null ? t.Category.Name : "Uncategorized",
                    CategoryIcon = t.Category != null ? t.Category.Icon : null,
                    Description = t.Description
                })
                .ToListAsync();

            var activeBudgets = await _db.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == currentUser.Id && b.StartDate <= now && b.EndDate >= now)
                .ToListAsync();

            var budgetSpendLookup = await _db.Transactions
                .Where(t => t.UserId == currentUser.Id && t.TransactionDate >= sixMonthsAgo && t.TransactionDate <= now && t.Type == 2)
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, Spent = g.Sum(x => x.Amount) })
                .ToListAsync();

            var spentByCategory = budgetSpendLookup.ToDictionary(x => x.CategoryId ?? 0, x => x.Spent);

            BudgetProgress = activeBudgets
                .Select(b => new BudgetProgressItem
                {
                    CategoryName = b.Category?.Name ?? "Uncategorized",
                    AmountBudgeted = b.Amount,
                    AmountSpent = spentByCategory.TryGetValue(b.CategoryId ?? 0, out var spent) ? spent : 0m,
                    Color = b.Category?.Color,
                    Icon = b.Category?.Icon
                })
                .OrderByDescending(x => x.AmountBudgeted)
                .ToList();
        }

        public class CategorySpending
        {
            public long CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class MonthlyTrendPoint
        {
            public string Label { get; set; } = string.Empty;
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
            public decimal Surplus => Income - Expense;
        }

        public class RecentTransactionItem
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public int Type { get; set; }
            public string? CategoryName { get; set; }
            public string? CategoryIcon { get; set; }
            public string? Description { get; set; }
        }

        public class BudgetProgressItem
        {
            public string CategoryName { get; set; } = string.Empty;
            public decimal AmountBudgeted { get; set; }
            public decimal AmountSpent { get; set; }
            public string? Color { get; set; }
            public string? Icon { get; set; }
            public decimal Percent => AmountBudgeted == 0 ? 0 : Math.Min(100, Math.Round((AmountSpent / AmountBudgeted) * 100, 0));
        }
    }
}


