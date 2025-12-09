namespace MoneyTrackerApp.DTOs;

public class AiSuggestionDto
{
    public long Id { get; set; }
    public string Suggestion { get; set; } = string.Empty;
    public string SuggestionType { get; set; } = "info"; // success, warning, info, danger
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
