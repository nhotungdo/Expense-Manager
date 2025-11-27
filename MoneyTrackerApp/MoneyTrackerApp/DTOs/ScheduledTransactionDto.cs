using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a scheduled transaction
/// </summary>
public class CreateScheduledTransactionDto
{
    [Required(ErrorMessage = "Account ID is required")]
    public long AccountId { get; set; }

    public long? CategoryId { get; set; }

    [Required(ErrorMessage = "Transaction type is required")]
    [Range(1, 2, ErrorMessage = "Invalid transaction type (1=Income, 2=Expense)")]
    public int TransactionType { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Frequency is required")]
    [RegularExpression("^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency must be Daily, Weekly, Monthly, or Yearly")]
    public string Frequency { get; set; } = null!;

    [Required(ErrorMessage = "Interval is required")]
    [Range(1, 365, ErrorMessage = "Interval must be between 1 and 365")]
    public int Interval { get; set; } = 1;

    [Required(ErrorMessage = "Start date is required")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }
}

/// <summary>
/// DTO for updating a scheduled transaction
/// </summary>
public class UpdateScheduledTransactionDto
{
    [Required(ErrorMessage = "Scheduled transaction ID is required")]
    public long Id { get; set; }

    public long? CategoryId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }

    [RegularExpression("^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency must be Daily, Weekly, Monthly, or Yearly")]
    public string? Frequency { get; set; }

    [Range(1, 365, ErrorMessage = "Interval must be between 1 and 365")]
    public int? Interval { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for scheduled transaction response
/// </summary>
public class ScheduledTransactionResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = null!;
    public int Interval { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextRunDate { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
