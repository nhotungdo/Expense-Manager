using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.DTOs.Report;

public class SummaryReportDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "VND";
}

public class CategoryBreakdownDto
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int TransactionCount { get; set; }
}

public class IncomeExpenseTrendDto
{
    public string Period { get; set; } = string.Empty; // "2024-01", "Q1 2024", etc.
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance => Income - Expense;
}

public class ExportReportRequest
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public string Format { get; set; } = "Excel"; // "Excel" or "PDF"

    public bool IncludeCategories { get; set; } = true;
    public bool IncludeBudgets { get; set; } = true;
    public bool IncludeCharts { get; set; } = true;
}

public class ReportFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Period { get; set; } // "monthly", "quarterly", "yearly"
}
