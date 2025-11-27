namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for net worth calculation and display
/// </summary>
public class NetWorthDto
{
    /// <summary>
    /// Total value of all active accounts included in net worth
    /// </summary>
    public decimal TotalAssets { get; set; }

    /// <summary>
    /// Total debt (negative credit card balances)
    /// </summary>
    public decimal TotalDebt { get; set; }

    /// <summary>
    /// Net worth = Total Assets - Total Debt
    /// </summary>
    public decimal NetWorth { get; set; }

    /// <summary>
    /// Breakdown by account type
    /// </summary>
    public List<NetWorthByTypeDto> ByAccountType { get; set; } = new List<NetWorthByTypeDto>();

    /// <summary>
    /// Breakdown by currency
    /// </summary>
    public List<NetWorthByCurrencyDto> ByCurrency { get; set; } = new List<NetWorthByCurrencyDto>();

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Net worth breakdown by account type
/// </summary>
public class NetWorthByTypeDto
{
    public int AccountType { get; set; }

    public string AccountTypeDisplay { get; set; } = null!;

    public decimal Balance { get; set; }

    public int Count { get; set; }
}

/// <summary>
/// Net worth breakdown by currency
/// </summary>
public class NetWorthByCurrencyDto
{
    public string Currency { get; set; } = null!;

    public decimal Balance { get; set; }

    public int Count { get; set; }
}
