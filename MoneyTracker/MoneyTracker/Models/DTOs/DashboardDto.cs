namespace MoneyTracker.Models.DTOs
{
    public class DashboardDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetWorth { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal MonthlySavings { get; set; }
        public List<CategorySpendingDto> ExpensesByCategory { get; set; } = new();
        public List<CategorySpendingDto> IncomeByCategory { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        public List<AiSuggestion> AiSuggestions { get; set; } = new();
    }

    public class DashboardStatsDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetIncome { get; set; }
        public int TransactionCount { get; set; }
    }

    public class RecentTransaction
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
