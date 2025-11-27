using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating a notification
/// </summary>
public class CreateNotificationDto
{
    [Required(ErrorMessage = "User ID is required")]
    public long UserId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(256, ErrorMessage = "Title must be less than 256 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1024, ErrorMessage = "Message must be less than 1024 characters")]
    public string Message { get; set; } = null!;

    [Required(ErrorMessage = "Type is required")]
    [StringLength(50, ErrorMessage = "Type must be less than 50 characters")]
    public string Type { get; set; } = null!;

    [StringLength(512, ErrorMessage = "Action URL must be less than 512 characters")]
    public string? ActionUrl { get; set; }
}

/// <summary>
/// DTO for notification response
/// </summary>
public class NotificationResponseDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for currency rate
/// </summary>
public class CurrencyRateDto
{
    public string FromCurrency { get; set; } = null!;
    public string ToCurrency { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO for currency conversion
/// </summary>
public class CurrencyConversionDto
{
    [Required(ErrorMessage = "From currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string FromCurrency { get; set; } = null!;

    [Required(ErrorMessage = "To currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string ToCurrency { get; set; } = null!;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}

/// <summary>
/// DTO for currency conversion result
/// </summary>
public class CurrencyConversionResultDto
{
    public string FromCurrency { get; set; } = null!;
    public string ToCurrency { get; set; } = null!;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime RateDate { get; set; }
}

/// <summary>
/// DTO for AI financial suggestion
/// </summary>
public class AiSuggestionDto
{
    public long Id { get; set; }
    public string SuggestionType { get; set; } = null!;
    public string Suggestion { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for generating AI suggestions
/// </summary>
public class GenerateAiSuggestionsDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
