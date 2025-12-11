namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTOs for User-Facing AI Features
/// </summary>

public class PlanRecommendationDto
{
    public string Message { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty; // "upgrade", "savings", "comparison"
    public int? RecommendedPackageId { get; set; }
    public decimal? PotentialSavings { get; set; }
    public string? ActionUrl { get; set; }
}

public class TransactionInsightDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? DownloadUrl { get; set; }
}

public class BillExplanationDto
{
    public decimal CurrentAmount { get; set; }
    public decimal PreviousAmount { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<BillChangeItemDto> Changes { get; set; } = new();
}

public class BillChangeItemDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SpendingForecastDto
{
    public decimal CurrentMonthlySpending { get; set; }
    public decimal ForecastedNextMonth { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool CanSetLimit { get; set; }
}

public class TransactionSearchRequestDto
{
    public string Query { get; set; } = string.Empty; // e.g., "Find me your May bill from last year"
}

public class TransactionSearchResultDto
{
    public List<TransactionSearchItemDto> Transactions { get; set; } = new();
    public string? DownloadUrl { get; set; }
}

public class TransactionSearchItemDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

