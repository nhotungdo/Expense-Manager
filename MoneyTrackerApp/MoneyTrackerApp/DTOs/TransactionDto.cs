using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a new transaction
/// </summary>
public class CreateTransactionDto
{
    [Required(ErrorMessage = "Account ID is required")]
    public long AccountId { get; set; }

    public long? CategoryId { get; set; }

    [Required(ErrorMessage = "Transaction type is required")]
    [Range(1, 3, ErrorMessage = "Invalid transaction type (1=Income, 2=Expense, 3=Transfer)")]
    public int TransactionType { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string Currency { get; set; } = null!;

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Transaction date is required")]
    public DateTime TransactionDate { get; set; }

    // For transfer transactions
    public long? PairedAccountId { get; set; }

    // For receipt scanning
    [StringLength(512, ErrorMessage = "Attachment URL must be less than 512 characters")]
    public string? AttachmentUrl { get; set; }
    public string? OcrText { get; set; }

    // For Recurring Transaction
    public bool IsRecurring { get; set; }
    public string? RecurringFrequency { get; set; }
    public int? RecurringInterval { get; set; }
    public DateTime? RecurringEndDate { get; set; }
}

/// <summary>
/// DTO for updating an existing transaction
/// </summary>
public class UpdateTransactionDto
{
    [Required(ErrorMessage = "Transaction ID is required")]
    public long Id { get; set; }

    public long? CategoryId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }

    [StringLength(512, ErrorMessage = "Note must be less than 512 characters")]
    public string? Note { get; set; }

    public DateTime? TransactionDate { get; set; }

    [StringLength(512, ErrorMessage = "Attachment URL must be less than 512 characters")]
    public string? AttachmentUrl { get; set; }
}

/// <summary>
/// DTO for transaction response
/// </summary>
public class TransactionResponseDto
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
    public string Currency { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime TransactionDate { get; set; }
    public long? PairedAccountId { get; set; }
    public string? PairedAccountName { get; set; }
    public long? PairedTransactionId { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? OcrText { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Display description for the transaction (combines CategoryName and Note intelligently)
    /// </summary>
    public string Description { get; set; } = null!;

    public string? WarningMessage { get; set; }
    
    // Spender details for Shared Wallets
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
}

/// <summary>
/// DTO for transaction list with filters
/// </summary>
public class TransactionFilterDto
{
    public long? AccountId { get; set; }
    public long? CategoryId { get; set; }
    public int? TransactionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SearchText { get; set; }
    public long? UserId { get; set; } // Added for Admin filtering
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for OCR receipt scanning
/// </summary>
public class OcrReceiptDto
{
    [Required(ErrorMessage = "Receipt image is required")]
    public string ImageBase64 { get; set; } = null!;

    public long? AccountId { get; set; }
    public long? CategoryId { get; set; }
}

/// <summary>
/// DTO for OCR result
/// </summary>
public class OcrResultDto
{
    public string RawText { get; set; } = null!;
    public string? MerchantName { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public decimal Confidence { get; set; }
}

public class SpendingContributionDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string? UserAvatar { get; set; }
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}
