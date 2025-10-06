namespace MoneyTracker.Models.DTOs
{
    public class SpendingAnalysisDto
    {
        public decimal TotalSpent { get; set; }
        public decimal AverageDailySpending { get; set; }
        public decimal AverageMonthlySpending { get; set; }
        public decimal HighestSpendingDay { get; set; }
        public DateTime? HighestSpendingDate { get; set; }
        public List<CategorySpendingDto> TopCategories { get; set; } = new();
        public List<DailySpendingDto> DailySpending { get; set; } = new();
        public List<WeeklySpendingDto> WeeklySpending { get; set; } = new();
        public SpendingPatternDto SpendingPattern { get; set; } = new();
    }

    public class IncomeAnalysisDto
    {
        public decimal TotalIncome { get; set; }
        public decimal AverageDailyIncome { get; set; }
        public decimal AverageMonthlyIncome { get; set; }
        public decimal HighestIncomeDay { get; set; }
        public DateTime? HighestIncomeDate { get; set; }
        public List<CategoryIncomeDto> TopCategories { get; set; } = new();
        public List<DailyIncomeDto> DailyIncome { get; set; } = new();
        public List<WeeklyIncomeDto> WeeklyIncome { get; set; } = new();
        public IncomePatternDto IncomePattern { get; set; } = new();
    }

    public class BudgetAnalysisDto
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal BudgetUtilization { get; set; }
        public string BudgetStatus { get; set; } = string.Empty;
        public List<CategoryBudgetDto> CategoryBudgets { get; set; } = new();
        public List<BudgetAlertDto> Alerts { get; set; } = new();
        public BudgetRecommendationDto Recommendations { get; set; } = new();
    }

    public class FinancialHealthDto
    {
        public decimal NetWorth { get; set; }
        public decimal SavingsRate { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public string HealthScore { get; set; } = string.Empty;
        public string HealthStatus { get; set; } = string.Empty;
        public List<HealthMetricDto> Metrics { get; set; } = new();
        public List<HealthRecommendationDto> Recommendations { get; set; } = new();
    }

    public class TrendAnalysisDto
    {
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public List<CategoryTrendDto> CategoryTrends { get; set; } = new();
        public TrendDirectionDto IncomeTrend { get; set; } = new();
        public TrendDirectionDto ExpenseTrend { get; set; } = new();
        public TrendDirectionDto SavingsTrend { get; set; } = new();
        public List<SeasonalPatternDto> SeasonalPatterns { get; set; } = new();
    }

    public class CategoryInsightsDto
    {
        public List<CategoryInsightDto> Insights { get; set; } = new();
        public List<CategoryComparisonDto> Comparisons { get; set; } = new();
        public List<CategoryAnomalyDto> Anomalies { get; set; } = new();
        public List<CategoryRecommendationDto> Recommendations { get; set; } = new();
    }

    public class ForecastDto
    {
        public List<MonthlyForecastDto> MonthlyForecasts { get; set; } = new();
        public decimal ProjectedIncome { get; set; }
        public decimal ProjectedExpenses { get; set; }
        public decimal ProjectedSavings { get; set; }
        public decimal ProjectedNetWorth { get; set; }
        public List<ForecastScenarioDto> Scenarios { get; set; } = new();
        public ForecastConfidenceDto Confidence { get; set; } = new();
    }

    // Supporting DTOs
    public class CategorySpendingDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class DailySpendingDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class WeeklySpendingDto
    {
        public int Week { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class SpendingPatternDto
    {
        public string MostSpentDay { get; set; } = string.Empty;
        public string MostSpentTime { get; set; } = string.Empty;
        public decimal WeekendSpending { get; set; }
        public decimal WeekdaySpending { get; set; }
        public List<HourlySpendingDto> HourlySpending { get; set; } = new();
    }

    public class HourlySpendingDto
    {
        public int Hour { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class CategoryIncomeDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class DailyIncomeDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class WeeklyIncomeDto
    {
        public int Week { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class IncomePatternDto
    {
        public string MostIncomeDay { get; set; } = string.Empty;
        public string MostIncomeTime { get; set; } = string.Empty;
        public decimal WeekendIncome { get; set; }
        public decimal WeekdayIncome { get; set; }
        public List<HourlyIncomeDto> HourlyIncome { get; set; } = new();
    }

    public class HourlyIncomeDto
    {
        public int Hour { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class CategoryBudgetDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BudgetAlertDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class BudgetRecommendationDto
    {
        public List<string> Recommendations { get; set; } = new();
        public decimal SuggestedBudgetAdjustment { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class HealthMetricDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class HealthRecommendationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Savings { get; set; }
        public decimal NetWorth { get; set; }
    }

    public class CategoryTrendDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<MonthlyCategoryTrendDto> MonthlyData { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty;
        public decimal TrendPercentage { get; set; }
    }

    public class MonthlyCategoryTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class TrendDirectionDto
    {
        public string Direction { get; set; } = string.Empty; // "up", "down", "stable"
        public decimal Percentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class SeasonalPatternDto
    {
        public string Season { get; set; } = string.Empty;
        public decimal AverageAmount { get; set; }
        public decimal Variance { get; set; }
        public string Pattern { get; set; } = string.Empty;
    }

    public class CategoryInsightDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Insight { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class CategoryComparisonDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal ChangePercentage { get; set; }
        public string Comparison { get; set; } = string.Empty;
    }

    public class CategoryAnomalyDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal Deviation { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryRecommendationDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
    }

    public class MonthlyForecastDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal ForecastedIncome { get; set; }
        public decimal ForecastedExpenses { get; set; }
        public decimal ForecastedSavings { get; set; }
        public decimal ForecastedNetWorth { get; set; }
        public decimal Confidence { get; set; }
    }

    public class ForecastScenarioDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal IncomeMultiplier { get; set; }
        public decimal ExpenseMultiplier { get; set; }
        public decimal ProjectedNetWorth { get; set; }
    }

    public class ForecastConfidenceDto
    {
        public decimal OverallConfidence { get; set; }
        public decimal IncomeConfidence { get; set; }
        public decimal ExpenseConfidence { get; set; }
        public string ConfidenceLevel { get; set; } = string.Empty;
        public List<string> Factors { get; set; } = new();
    }
}
