using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.DTOs.AI;

public class AiSuggestionDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "budget", "spending", "saving", "category"
    public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High"
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AiSuggestionRequest
{
    public string? CategoryId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
}

public class AiSuggestionResponse
{
    public List<AiSuggestionDto> Suggestions { get; set; } = new();
    public int TotalCount { get; set; }
    public DateTime GeneratedAt { get; set; }
}
