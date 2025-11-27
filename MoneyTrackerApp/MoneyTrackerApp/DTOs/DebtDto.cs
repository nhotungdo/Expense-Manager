using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a debt
/// </summary>
public class CreateDebtDto
{
    [Required(ErrorMessage = "Debt type is required")]
    [Range(1, 2, ErrorMessage = "Invalid debt type (1=I owe them, 2=They owe me)")]
    public int DebtType { get; set; }

    [Required(ErrorMessage = "Debt name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Debt name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Person name must be less than 100 characters")]
    public string? PersonName { get; set; }

    [Required(ErrorMessage = "Initial amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Initial amount must be greater than 0")]
    public decimal InitialAmount { get; set; }

    [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
    public decimal InterestRate { get; set; } = 0;

    [Required(ErrorMessage = "Start date is required")]
    public DateOnly StartDate { get; set; }

    public DateOnly? DueDate { get; set; }
}

/// <summary>
/// DTO for updating a debt
/// </summary>
public class UpdateDebtDto
{
    [Required(ErrorMessage = "Debt ID is required")]
    public long Id { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Debt name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [StringLength(100, ErrorMessage = "Person name must be less than 100 characters")]
    public string? PersonName { get; set; }

    [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
    public decimal? InterestRate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Range(1, 4, ErrorMessage = "Invalid status")]
    public int? Status { get; set; }
}

/// <summary>
/// DTO for recording a debt payment
/// </summary>
public class RecordDebtPaymentDto
{
    [Required(ErrorMessage = "Debt ID is required")]
    public long DebtId { get; set; }

    [Required(ErrorMessage = "Transaction ID is required")]
    public long TransactionId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }
}

/// <summary>
/// DTO for debt response
/// </summary>
public class DebtResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int DebtType { get; set; }
    public string DebtTypeDisplay { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? PersonName { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalWithInterest { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public int? DaysRemaining { get; set; }
    public bool IsOverdue { get; set; }
    public int Status { get; set; }
    public string StatusDisplay { get; set; } = null!;
    public decimal PercentagePaid { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DebtPaymentDto> Payments { get; set; } = new();
}

/// <summary>
/// DTO for debt payment
/// </summary>
public class DebtPaymentDto
{
    public long Id { get; set; }
    public long DebtId { get; set; }
    public long TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for debt summary
/// </summary>
public class DebtSummaryDto
{
    public int TotalDebts { get; set; }
    public int ActiveDebts { get; set; }
    public decimal TotalIOwe { get; set; }
    public decimal TotalTheyOweMe { get; set; }
    public decimal NetDebt { get; set; }
    public decimal TotalInterest { get; set; }
    public List<DebtResponseDto> IOweThem { get; set; } = new();
    public List<DebtResponseDto> TheyOweMe { get; set; } = new();
}
