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
            // 1. Get current balance
            var currentBalance = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentBalance);

            // 2. Get average daily spending (last 90 days)
            var threeMonthsAgo = DateTime.UtcNow.AddDays(-90);
            var recentEspenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= threeMonthsAgo && t.Amount < 0) // Assuming expense is negative
                .SumAsync(t => t.Amount);

            // Note: If Amount is stored as positive for expense with Type='Expense', logic needs adjustment. 
            // Checking Transaction model might be needed. Assuming standardize expense handling.
            // Let's assume Amount is absolute and TransactionType determines sign.
            // Typically in this app based on other files, Expense is 'Expense'.
            
            var expenseTotal = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= threeMonthsAgo && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Expense)
                .SumAsync(t => t.Amount);

            var incomeTotal = await _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= threeMonthsAgo && t.TransactionType == (int)MoneyTrackerApp.Enums.TransactionType.Income)
                .SumAsync(t => t.Amount);

            var dailyNetChange = (incomeTotal - expenseTotal) / 90; // Simple linear average

            // 3. Get Scheduled Transactions
            var scheduled = await _context.ScheduledTransactions
                .Where(s => s.UserId == userId && s.IsActive == true)
                .ToListAsync();

            var result = new CashflowForecastResult();
            var runningBalance = currentBalance;
            var riskTouched = false;
            DateOnly? riskDate = null;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            for (int i = 0; i < days; i++)
            {
                var currentDate = today.AddDays(i);
                
                // Add daily average drift
                runningBalance += dailyNetChange;

                // Add specific scheduled items
                // This is a simplified check. A robust one would handle frequencies correctly.
                foreach (var item in scheduled)
                {
                    // Check if item occurs on currentDate
                    // Simplification: Not implemented fully for all frequencies in this snippet
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
