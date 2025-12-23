using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Pages.Budgets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public decimal TotalBudget { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public decimal Percentage { get; set; } = 0;
        public decimal ProjectedSpent { get; set; } = 0;

        public List<CategorySpendingDto> CategorySpendings { get; set; } = new();
        public List<DailySpendingDto> DailySpendings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return;

            // 1. Get Current Month Range
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 2. Fetch Global Monthly Budget (CategoryId == null represents global)
            var budget = await _context.Budgets
                .Where(b => b.UserId == userId 
                         && b.CategoryId == null 
                         && b.Period == 2 
                         && b.StartDate <= now 
                         && b.EndDate >= now)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            TotalBudget = budget?.Amount ?? 0;

            // 3. Fetch Expenses for this month
            var expenses = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId 
                         && t.TransactionType == 2 // Expense
                         && t.TransactionDate >= startOfMonth 
                         && t.TransactionDate <= endOfMonth)
                .ToListAsync();

            TotalSpent = expenses.Sum(t => t.Amount);

            // 4. Calculate Percentage and Projection
            if (TotalBudget > 0)
            {
                Percentage = (TotalSpent / TotalBudget) * 100;
            }

            // Simple Linear Projection: Daily Avg * Days in Month
            int daysPassed = now.Day;
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            if (daysPassed > 0)
            {
                decimal dailyAvg = TotalSpent / daysPassed;
                ProjectedSpent = dailyAvg * daysInMonth;
            }

            // 5. Populate Category Spending
            var groupedByCategory = expenses
                .GroupBy(t => t.CategoryId)
                .Select(g => new 
                { 
                    CategoryId = g.Key, 
                    Amount = g.Sum(t => t.Amount),
                    CategoryObj = g.First().Category
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            foreach (var item in groupedByCategory)
            {
                CategorySpendings.Add(new CategorySpendingDto
                {
                    Name = item.CategoryObj?.Name ?? "Khác",
                    Amount = item.Amount,
                    Color = item.CategoryObj?.Color ?? "#9CA3AF", // Default gray
                    Icon = item.CategoryObj?.Icon ?? "fas fa-coins"
                });
            }

            // 6. Populate Daily Spending (Cumulative)
            decimal runningTotal = 0;
            for (int day = 1; day <= daysInMonth; day++)
            {
                if (day > now.Day) break;

                var date = new DateTime(now.Year, now.Month, day);
                var dailySum = expenses.Where(t => t.TransactionDate.Date == date.Date).Sum(t => t.Amount);
                runningTotal += dailySum;

                DailySpendings.Add(new DailySpendingDto
                {
                    Date = date.ToString("dd/MM"),
                    Amount = dailySum,
                    Cumulative = runningTotal
                });
            }
        }

        public async Task<IActionResult> OnPostUpdateBudgetAsync([FromBody] UpdateBudgetRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return new JsonResult(new { success = false, message = "User not found" });

            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Find existing budget for this month
                var budget = await _context.Budgets
                    .Where(b => b.UserId == userId
                             && b.CategoryId == null
                             && b.Period == 2
                             && b.StartDate <= now
                             && b.EndDate >= now)
                    .FirstOrDefaultAsync();

                if (budget == null)
                {
                    // Create new
                    budget = new Budget
                    {
                        UserId = userId,
                        CategoryId = null, // Global
                        AccountId = null,
                        Amount = request.Amount,
                        Period = 2, // Monthly
                        StartDate = startOfMonth,
                        EndDate = endOfMonth,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Budgets.Add(budget);
                }
                else
                {
                    // Update existing
                    budget.Amount = request.Amount;
                    budget.UpdatedAt = DateTime.UtcNow;
                    _context.Budgets.Update(budget);
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, newLimit = budget.Amount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class UpdateBudgetRequest
        {
            public decimal Amount { get; set; }
            public string? CapType { get; set; }
        }

        public class CategorySpendingDto
        {
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public string Color { get; set; }
            public string Icon { get; set; }
        }

        public class DailySpendingDto
        {
            public string Date { get; set; }
            public decimal Amount { get; set; }
            public decimal Cumulative { get; set; }
        }
    }
}
