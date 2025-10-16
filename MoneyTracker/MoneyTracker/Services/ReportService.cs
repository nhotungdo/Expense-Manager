using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class ReportService : IReportService
    {
        private readonly ExpenseManagerContext _db;

        public ReportService(ExpenseManagerContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(int monthsBack = 6)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-monthsBack + 1);
            var startDateOnly = DateOnly.FromDateTime(start);

            var expenses = await _db.Expenses
                .Where(e => e.ExpenseDate >= startDateOnly)
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var incomes = await _db.Incomes
                .Where(i => i.IncomeDate >= startDateOnly)
                .GroupBy(i => new { i.IncomeDate.Year, i.IncomeDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var months = Enumerable.Range(0, monthsBack)
                .Select(offset => start.AddMonths(offset))
                .Select(d => new MonthlyTotal
                {
                    Year = d.Year,
                    Month = d.Month,
                    TotalExpense = expenses.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0,
                    TotalIncome = incomes.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0,
                })
                .ToList();

            return months;
        }
    }
}

