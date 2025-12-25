using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public class FinancialAnalysisService : IFinancialAnalysisService
    {
        private readonly ExpenseManagerContext _context;

        public FinancialAnalysisService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<CashflowForecastResult> GetCashflowForecastAsync(long userId, int days = 30)
        {
            var result = new CashflowForecastResult();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // 1. Get current balance
            var currentBalance = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentBalance);

            // 2. Get average daily spending (last 90 days)
            var threeMonthsAgo = DateTime.UtcNow.AddDays(-90);
            
            var expenseTotal = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= threeMonthsAgo && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Expense)
                .SumAsync(t => t.Amount);

            var incomeTotal = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= threeMonthsAgo && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Income)
                .SumAsync(t => t.Amount);

            // Calculate daily drift (Linear Regression Lite)
            // If user consistently saves, this will be positive. If they overspend, negative.
            var dailyNetChange = (incomeTotal - expenseTotal) / 90;

            // 3. Get Scheduled Transactions
            var scheduled = await _context.ScheduledTransactions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            var runningBalance = currentBalance;
            var riskTouched = false;
            DateOnly? riskDate = null;

            for (int i = 0; i < days; i++)
            {
                var currentDate = today.AddDays(i);
                
                // Add daily average drift (organic spending/income)
                // We use 50% of historical drift as 'unexpected' spending/income, 
                // assuming fixed bills are covered by scheduled transactions if they exist.
                // If the user has NO scheduled transactions, we lean 100% on historical.
                decimal dailyDrift = scheduled.Any() ? dailyNetChange * 0.5m : dailyNetChange;
                runningBalance += dailyDrift;

                // Add specific scheduled items matching this date
                foreach (var item in scheduled)
                {
                    bool isDue = false;
                    
                    // Logic to check if item.NextRunDate matches currentDate
                    // Since specific logic for NextRunDate update is complex, we simulate it based on Frequency
                    if (item.NextRunDate == currentDate)
                    {
                         isDue = true;
                    }
                    else if (item.NextRunDate < currentDate) // Determine if it would recur on this day
                    {
                        // Calculate days diff
                        // This is simplified. Ideally we project specific dates.
                        // Let's assume strict NextRunDate usage from DB is hard without simulating the update.
                        // Instead, we check Frequency:
                        
                        var daysDiff = currentDate.DayNumber - item.NextRunDate.DayNumber;
                        if (daysDiff > 0)
                        {
                            if (item.Frequency == "Daily" && daysDiff % item.Interval == 0) isDue = true;
                            if (item.Frequency == "Weekly" && daysDiff % (7 * item.Interval) == 0) isDue = true;
                            // Monthly is harder with DayNumber, simply checking Day of Month
                            if (item.Frequency == "Monthly" && currentDate.Day == item.NextRunDate.Day) 
                            {
                                int monthDiff = (currentDate.Year - item.NextRunDate.Year) * 12 + currentDate.Month - item.NextRunDate.Month;
                                if (monthDiff > 0 && monthDiff % item.Interval == 0) isDue = true;
                            }
                        }
                    }

                    if (isDue)
                    {
                        if (item.TransactionType == 1) runningBalance += item.Amount; // Income
                        else runningBalance -= item.Amount; // Expense
                    }
                }

                result.ForecastPoints.Add(new ForecastPoint
                {
                    Date = currentDate,
                    PredictedBalance = runningBalance
                });

                if (!riskTouched && runningBalance < 0)
                {
                    riskTouched = true;
                    riskDate = currentDate;
                }
            }

            result.IsRiskZoneTouched = riskTouched;
            result.RiskDate = riskDate;
            result.CurrentSpendingRate = expenseTotal / 3; // Monthly average

            return result;
        }

        public async Task<FinancialHealthScoreResult> GetFinancialHealthScoreAsync(long userId)
        {
            // 1. Savings / Income Ratio
            // Income (Last 30 days)
            var MonthAgo = DateTime.UtcNow.AddDays(-30);
            var lastMonthIncome = await _context.Transactions
                 .Where(t => t.UserId == userId && t.TransactionDate >= MonthAgo && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Income)
                 .SumAsync(t => t.Amount);

            // Savings (Last 30 days contributions to SavingsGoals)
            // Or simple diff? Let's use SavingsTransactions
            var lastMonthSavings = await _context.SavingsTransactions
                .Where(st => st.Transaction.UserId == userId && st.Transaction.TransactionDate >= MonthAgo)
                .SumAsync(st => st.Amount);

            decimal savingsRatio = lastMonthIncome > 0 ? (lastMonthSavings / lastMonthIncome) : 0;
            
            // 2. Debt / Asset Ratio
            var totalDebt = await _context.Debts
                .Where(d => d.UserId == userId)
                .SumAsync(d => d.InitialAmount - (decimal)d.AmountPaid); // Cast if needed

            var totalAssets = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentBalance);
            // Add Property Assets
            var propertyAssets = await _context.Assets
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentValue);
            
            totalAssets += propertyAssets;

            decimal debtRatio = totalAssets > 0 ? (totalDebt / totalAssets) : (totalDebt > 0 ? 1 : 0);

            // 3. Budget Compliance
            // Check active budgets for this month
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.StartDate <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            decimal complianceScore = 1; // Default perfect
            if (budgets.Any())
            {
                int compliantCount = 0;
                foreach (var budget in budgets)
                {
                    // Calculate actual spend for this budget category
                    var spent = await _context.Transactions
                        .Where(t => t.UserId == userId && t.CategoryId == budget.CategoryId 
                                    && t.TransactionDate >= budget.StartDate && t.TransactionDate <= budget.EndDate
                                    && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Expense)
                        .SumAsync(t => t.Amount);
                    
                    if (spent <= budget.Amount) compliantCount++;
                }
                complianceScore = (decimal)compliantCount / budgets.Count;
            }

            // Calculation (Scale 1000)
            // Weightings: Savings (40%), Debt (30%), Compliance (30%)
            // Savings: target 20% -> 100% score. 
            // Debt: target 0% -> 100% score. 
            
            double score = 0;
            
            // Savings Score (0 to 400)
            double sScore = (double)savingsRatio * 5; // if 0.2 (20%) -> 1.0 * 400.
            if (sScore > 1) sScore = 1;
            score += sScore * 400;

            // Debt Score (0 to 300)
            // Low debt ratio is good.
            double dScore = 1 - (double)debtRatio;
            if (dScore < 0) dScore = 0;
            score += dScore * 300;

            // Compliance Score (0 to 300)
            score += (double)complianceScore * 300;
            
            var result = new FinancialHealthScoreResult
            {
                TotalScore = (int)score,
                SavingsIncomeRatio = savingsRatio,
                DebtAssetRatio = debtRatio,
                BudgetCompliance = complianceScore
            };

            if (score >= 800) result.HealthStatus = "Excellent";
            else if (score >= 600) result.HealthStatus = "Good";
            else if (score >= 400) result.HealthStatus = "Fair";
            else result.HealthStatus = "Needs Improvement";

            return result;
        }
    }
}
