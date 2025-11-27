using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a budget
/// </summary>
public class CreateBudgetDto
{
    public long? CategoryId { get; set; }
    public long? AccountId { get; set; }

    [Required(ErrorMessage = "Budget amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Period is required")]
    [Range(1, 4, ErrorMessage = "Invalid period (1=Daily, 2=Weekly, 3=Monthly, 4=Yearly)")]
    public int Period { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }
}

/// <summary>
/// DTO for updating a budget
/// </summary>
public class UpdateBudgetDto
{
    [Required(ErrorMessage = "Budget ID is required")]
    public long Id { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal? Amount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// DTO for budget response
/// </summary>
public class BudgetResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public long? AccountId { get; set; }
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
    public int Period { get; set; }
    public string PeriodDisplay { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentageUsed { get; set; }
    public bool IsOverBudget { get; set; }
    public bool IsNearLimit { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for budget summary
/// </summary>
public class BudgetSummaryDto
{
    public int TotalBudgets { get; set; }
    public int OverBudgetCount { get; set; }
    public int NearLimitCount { get; set; }
    public decimal TotalBudgeted { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRemaining { get; set; }
    public List<BudgetResponseDto> Budgets { get; set; } = new();
}

/// <summary>
/// DTO for budget alert
/// </summary>
public class BudgetAlertDto
{
    public long BudgetId { get; set; }
    public string BudgetName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal Spent { get; set; }
    public decimal PercentageUsed { get; set; }
    public string AlertType { get; set; } = null!; // "Near Limit" or "Over Budget"
    public string Message { get; set; } = null!;
}
