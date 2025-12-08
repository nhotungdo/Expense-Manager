using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for generating a report
/// </summary>
public class GenerateReportDto
{
    [Required(ErrorMessage = "Report type is required")]
    [Range(1, 6, ErrorMessage = "Invalid report type")]
    public int ReportType { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    public long? AccountId { get; set; }
    public long? CategoryId { get; set; }

    [Required(ErrorMessage = "File format is required")]
    [Range(1, 4, ErrorMessage = "Invalid file format (1=PDF, 2=Excel, 3=CSV, 4=JSON)")]
    public int FileFormat { get; set; }
}

/// <summary>
/// DTO for cash flow report
/// </summary>
public class CashFlowReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetCashFlow { get; set; }
    public List<CashFlowItemDto> IncomeItems { get; set; } = new();
    public List<CashFlowItemDto> ExpenseItems { get; set; } = new();
    public List<DailyCashFlowDto> DailyBreakdown { get; set; } = new();
}

/// <summary>
/// DTO for cash flow item
/// </summary>
public class CashFlowItemDto
{
    public string CategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for daily cash flow
/// </summary>
public class DailyCashFlowDto
{
    public DateTime Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetFlow { get; set; }
}

/// <summary>
/// DTO for monthly trend report
/// </summary>
public class MonthlyTrendReportDto
{
    public int Year { get; set; }
    public List<MonthlyDataDto> MonthlyData { get; set; } = new();
    public decimal AverageIncome { get; set; }
    public decimal AverageExpense { get; set; }
    public string Trend { get; set; } = null!; // "Increasing", "Decreasing", "Stable"
}

/// <summary>
/// DTO for monthly data
/// </summary>
public class MonthlyDataDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetIncome { get; set; }
    public decimal SavingsRate { get; set; }
}

/// <summary>
/// DTO for category breakdown report
/// </summary>
public class CategoryBreakdownReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CategoryBreakdownItemDto> IncomeCategories { get; set; } = new();
    public List<CategoryBreakdownItemDto> ExpenseCategories { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}

/// <summary>
/// DTO for category breakdown item
/// </summary>
public class CategoryBreakdownItemDto
{
    public string CategoryName { get; set; } = null!;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// DTO for dashboard overview
/// </summary>
public class DashboardOverviewDto
{
    public decimal CurrentBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlySavings { get; set; }
    public decimal SavingsRate { get; set; }
    public CashFlowChartDto CashFlowChart { get; set; } = new();
    public List<CategoryPieChartDto> ExpensePieChart { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    public List<BudgetAlertDto> BudgetAlerts { get; set; } = new();
    public List<SavingsGoalResponseDto> SavingsGoals { get; set; } = new();
}

/// <summary>
/// DTO for cash flow chart
/// </summary>
public class CashFlowChartDto
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> IncomeData { get; set; } = new();
    public List<decimal> ExpenseData { get; set; } = new();
}

/// <summary>
/// DTO for pie chart
/// </summary>
public class CategoryPieChartDto
{
    public string Label { get; set; } = null!;
    public decimal Value { get; set; }
    public string Color { get; set; } = null!;
}

/// <summary>
/// DTO for recent transaction
/// </summary>
public class RecentTransactionDto
{
    public long Id { get; set; }
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Type { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public string? CategoryName { get; set; }
    public string? AccountName { get; set; }
}
