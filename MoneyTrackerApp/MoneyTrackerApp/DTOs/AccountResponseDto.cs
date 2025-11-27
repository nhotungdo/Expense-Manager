namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for returning account/wallet details in API responses
/// </summary>
public class AccountResponseDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int AccountType { get; set; }

    public string AccountTypeDisplay { get; set; } = null!;

    public decimal InitialBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; }

    public bool IncludeInTotal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int SharedCount { get; set; }

    public bool IsBankLinked { get; set; }
}
