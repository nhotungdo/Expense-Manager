namespace MoneyTracker.Models.DTOs
{
    public class AiSuggestionDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Suggestion { get; set; } = string.Empty;
        public string SuggestionType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAiSuggestionDto
    {
        public string Suggestion { get; set; } = string.Empty;
        public string SuggestionType { get; set; } = "Financial Advice";
    }
}