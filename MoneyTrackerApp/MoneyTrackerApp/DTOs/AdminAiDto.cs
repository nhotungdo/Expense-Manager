namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTOs for Admin AI Features
/// </summary>

public class ChurnPredictionDto
{
    public long UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int RiskPercentage { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // "low", "medium", "high"
    public List<string> RiskFactors { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public DateTime LastLoginDate { get; set; }
    public int DaysSinceLastLogin { get; set; }
}

public class FraudDetectionDto
{
    public string AlertType { get; set; } = string.Empty; // "trial_abuse", "card_abuse", "suspicious_pattern"
    public string Message { get; set; } = string.Empty;
    public int AffectedAccountCount { get; set; }
    public List<FraudAccountDto> AffectedAccounts { get; set; } = new();
    public DateTime DetectedAt { get; set; }
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high", "critical"
    public bool AutoBlocked { get; set; }
}

public class FraudAccountDto
{
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? CardLastFour { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBlocked { get; set; }
}

public class NaturalLanguageQueryDto
{
    public string Query { get; set; } = string.Empty; // Vietnamese query
    public string QueryType { get; set; } = string.Empty; // "revenue_comparison", "top_customers", etc.
}

public class NaturalLanguageResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string? ChartType { get; set; } // "bar", "line", "pie", "table"
    public object? ChartData { get; set; }
    public string? Insights { get; set; }
    public List<DataRowDto>? DataRows { get; set; }
}

public class DataRowDto
{
    public Dictionary<string, object> Values { get; set; } = new();
}

public class RevenueComparisonDto
{
    public string Period { get; set; } = string.Empty;
    public decimal CurrentPeriodRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal GrowthPercentage { get; set; }
    public string? PrimaryGrowthSource { get; set; }
    public List<RevenueByRegionDto>? RevenueByRegion { get; set; }
}

public class RevenueByRegionDto
{
    public string Region { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TopCustomersDto
{
    public List<CustomerSpendingDto> Customers { get; set; } = new();
}

public class CustomerSpendingDto
{
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalSpending { get; set; }
    public bool HasActiveSubscription { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

