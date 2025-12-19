namespace MoneyTrackerApp.DTOs;

/// <summary>
/// Dashboard Analytics DTO for pie/doughnut charts
/// </summary>
public class DashboardAnalyticsDto
{
    public List<CategorySpendingDto> CategorySpending { get; set; } = new();
    public List<IncomeSourceDto> IncomeSource { get; set; } = new();
    public List<TransactionTypeDto> TransactionType { get; set; } = new();
    public List<WalletDistributionDto> WalletDistribution { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Category spending breakdown
/// </summary>
public class CategorySpendingDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Income source breakdown
/// </summary>
public class IncomeSourceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Transaction type breakdown
/// </summary>
public class TransactionTypeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Wallet distribution breakdown
/// </summary>
public class WalletDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}
