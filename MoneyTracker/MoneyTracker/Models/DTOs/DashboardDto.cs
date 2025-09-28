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
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public Dictionary<string, decimal> IncomeByCategory { get; set; } = new();
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        public List<AiSuggestion> AiSuggestions { get; set; } = new();
    }

    public class MonthlyTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Savings { get; set; }
    }

    public class RecentTransaction
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
