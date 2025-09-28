namespace MoneyTracker.Models.DTOs
{
    public class AiSuggestionDto
    {
        public long Id { get; set; }
        public string Suggestion { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Budget", "Spending", "Investment", etc.
        public long UserId { get; set; }
        public bool IsRead { get; set; }
        public bool IsApplied { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAiSuggestionDto
    {
        public string Suggestion { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long UserId { get; set; }
    }

    public class AiSuggestionFilterDto
    {
        public long? UserId { get; set; }
        public string? Type { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsApplied { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
