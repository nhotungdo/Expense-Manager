using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetSuggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var suggestions = await _aiService.GetSuggestionsAsync(userId.Value);
            var suggestionDtos = suggestions.Select(s => new
            {
                Id = s.Id,
                Suggestion = s.Suggestion,
                CreatedAt = s.CreatedAt
            });

            return Ok(suggestionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI suggestions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate-suggestion")]
    public async Task<ActionResult<dynamic>> GenerateSuggestion([FromBody] GenerateSuggestionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var suggestion = await _aiService.GenerateSuggestionAsync(userId.Value, request.SuggestionType);
            var suggestionDto = new
            {
                Id = suggestion.Id,
                Suggestion = suggestion.Suggestion,
                CreatedAt = suggestion.CreatedAt
            };

            return Ok(suggestionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestion");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("budget-suggestions")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetBudgetSuggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var suggestions = await _aiService.GetBudgetSuggestionsAsync(userId.Value);
            var suggestionDtos = suggestions.Select(s => new
            {
                Id = s.Id,
                Suggestion = s.Suggestion,
                CreatedAt = s.CreatedAt
            });

            return Ok(suggestionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget suggestions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("spending-suggestions")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetSpendingSuggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var suggestions = await _aiService.GetSpendingSuggestionsAsync(userId.Value);
            var suggestionDtos = suggestions.Select(s => new
            {
                Id = s.Id,
                Suggestion = s.Suggestion,
                CreatedAt = s.CreatedAt
            });

            return Ok(suggestionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spending suggestions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("savings-suggestions")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetSavingsSuggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var suggestions = await _aiService.GetSavingsSuggestionsAsync(userId.Value);
            var suggestionDtos = suggestions.Select(s => new
            {
                Id = s.Id,
                Suggestion = s.Suggestion,
                CreatedAt = s.CreatedAt
            });

            return Ok(suggestionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting savings suggestions");
            return StatusCode(500, "Internal server error");
        }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}

public class GenerateSuggestionRequest
{
    public string SuggestionType { get; set; } = string.Empty;
}
