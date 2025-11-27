namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for returning account/wallet summary (minimal info for lists)
/// </summary>
public class AccountSummaryDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int AccountType { get; set; }

    public string AccountTypeDisplay { get; set; } = null!;

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; }

    public bool IncludeInTotal { get; set; }
}
