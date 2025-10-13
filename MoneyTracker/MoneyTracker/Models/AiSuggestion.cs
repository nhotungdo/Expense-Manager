namespace MoneyTracker.Models;

public partial class AiSuggestion
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Suggestion { get; set; } = null!;

    public string SuggestionType { get; set; } = "Financial Advice";

    public bool IsRead { get; set; } = false;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
