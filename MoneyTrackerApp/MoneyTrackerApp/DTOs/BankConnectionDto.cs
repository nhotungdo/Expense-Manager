using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for linking a bank account
/// </summary>
public class LinkBankAccountDto
{
    [Required(ErrorMessage = "Provider is required")]
    [StringLength(50, ErrorMessage = "Provider must be less than 50 characters")]
    public string Provider { get; set; } = null!;

    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = null!;

    [Required(ErrorMessage = "Account ID is required")]
    public long AccountId { get; set; }

    [StringLength(100, ErrorMessage = "Item ID must be less than 100 characters")]
    public string? ItemId { get; set; }
}

/// <summary>
/// DTO for returning bank connection details
/// </summary>
public class BankConnectionResponseDto
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public string? ItemId { get; set; }

    public DateTime? LastSync { get; set; }

    public string? SyncStatus { get; set; }

    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// DTO for syncing bank transactions
/// </summary>
public class SyncBankTransactionsDto
{
    [Required(ErrorMessage = "Bank connection ID is required")]
    public long BankConnectionId { get; set; }

    public int TransactionCount { get; set; }

    public DateTime? SyncedAt { get; set; }

    public string Status { get; set; } = null!;
}
