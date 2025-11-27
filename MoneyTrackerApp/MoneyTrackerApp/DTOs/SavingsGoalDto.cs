using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a savings goal
/// </summary>
public class CreateSavingsGoalDto
{
    [Required(ErrorMessage = "Goal name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Goal name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Target amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
    public decimal TargetAmount { get; set; }

    public DateOnly? TargetDate { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }
}

/// <summary>
/// DTO for updating a savings goal
/// </summary>
public class UpdateSavingsGoalDto
{
    [Required(ErrorMessage = "Goal ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Goal name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
    public decimal? TargetAmount { get; set; }

    public DateOnly? TargetDate { get; set; }

    [StringLength(50, ErrorMessage = "Icon must be less than 50 characters")]
    public string? Icon { get; set; }

    [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }

    public int? Status { get; set; }
}

/// <summary>
/// DTO for adding money to savings goal
/// </summary>
public class AddToSavingsDto
{
    [Required(ErrorMessage = "Savings goal ID is required")]
    public long SavingsGoalId { get; set; }

    [Required(ErrorMessage = "Transaction ID is required")]
    public long TransactionId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }
}

/// <summary>
/// DTO for savings goal response
/// </summary>
public class SavingsGoalResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageCompleted { get; set; }
    public DateOnly? TargetDate { get; set; }
    public int? DaysRemaining { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Status { get; set; }
    public string StatusDisplay { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SavingsTransactionDto> Transactions { get; set; } = new();
}

/// <summary>
/// DTO for savings transaction
/// </summary>
public class SavingsTransactionDto
{
    public long Id { get; set; }
    public long SavingsGoalId { get; set; }
    public long TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for savings summary
/// </summary>
public class SavingsSummaryDto
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalSavedAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public decimal OverallPercentage { get; set; }
    public List<SavingsGoalResponseDto> Goals { get; set; } = new();
}
