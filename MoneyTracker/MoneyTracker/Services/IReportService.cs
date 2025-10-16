using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyTracker.Services
{
    public class MonthlyTotal
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public interface IReportService
    {
        Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(int monthsBack = 6);
    }
}

