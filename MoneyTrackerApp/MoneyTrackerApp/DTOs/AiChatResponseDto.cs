namespace MoneyTrackerApp.DTOs;

public class AiChatResponseDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class AiChatRequestDto
{
    public string Message { get; set; } = string.Empty;
}

public class AiInsightDto
{
    public string Title { get; set; } = string.Empty;
    public string Insight { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, warning, success, danger
    public string ActionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AiCashflowForecastDto
{
    public decimal CurrentBalance { get; set; }
    public decimal ProjectedBalance { get; set; }
    public decimal DailyBurnRate { get; set; }
    public int DaysRemaining { get; set; }
    public string Forecast { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // success, warning, danger
    public decimal MonthIncome { get; set; }
    public decimal MonthExpense { get; set; }
    public decimal UpcomingExpenses { get; set; }
}

public class GenerateAiSuggestionsDto
{
    public string? Context { get; set; }
}
