namespace MoneyTrackerApp.DTOs;

public class AutomationConditionDto
{
    public int? TransactionType { get; set; }
    public long? CategoryId { get; set; }
    public decimal? AmountThreshold { get; set; }
    public string? Operator { get; set; } // >, <, ==
}

public class AutomationActionDto
{
    public string Type { get; set; } // Notify, Transfer, Block
    public string? Message { get; set; }
    public long? TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
}
