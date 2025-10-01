namespace MoneyTracker.Models.DTOs
{
    public class AiSuggestionDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}