using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IFinancialAnalysisService
    {
        Task<CashflowForecastResult> GetCashflowForecastAsync(long userId, int days = 30);
        Task<FinancialHealthScoreResult> GetFinancialHealthScoreAsync(long userId);
    }

    public class CashflowForecastResult
    {
        public List<ForecastPoint> ForecastPoints { get; set; } = new List<ForecastPoint>();
        public bool IsRiskZoneTouched { get; set; }
        public DateOnly? RiskDate { get; set; }
        public decimal CurrentSpendingRate { get; set; }
    }

    public class ForecastPoint
    {
        public DateOnly Date { get; set; }
        public decimal PredictedBalance { get; set; }
    }

    public class FinancialHealthScoreResult
    {
        public int TotalScore { get; set; } // 0-1000
        public decimal SavingsIncomeRatio { get; set; }
        public decimal DebtAssetRatio { get; set; }
        public decimal BudgetCompliance { get; set; }
        public string HealthStatus { get; set; } = "Unknown"; // Healthy, Weak, etc.
    }
}
