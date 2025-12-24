using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for group member with detailed statistics
/// </summary>
public class GroupMemberDetailDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "Member";
    public bool IsActive { get; set; } = true;
    public int TransactionCount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public DateTime JoinedAt { get; set; }
}

/// <summary>
/// DTO for group category with statistics
/// </summary>
public class GroupCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = "fas fa-tag";
    public string Color { get; set; } = "#94a3b8";
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal? BudgetLimit { get; set; }
}

/// <summary>
/// DTO for group statistics
/// </summary>
public class GroupStatisticsDto
{
    public decimal TotalExpenses { get; set; }
    public decimal AverageExpense { get; set; }
    public double ExpenseTrend { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

/// <summary>
/// DTO for group budget
/// </summary>
public class GroupBudgetDto
{
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
}

/// <summary>
/// DTO for group budget alert
/// </summary>
public class GroupBudgetAlertDto
{
    public int Id { get; set; }
    public string Severity { get; set; } = "info";
    public string Icon { get; set; } = "fas fa-info-circle";
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
}
