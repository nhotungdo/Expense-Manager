using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// API Controller for AI Financial Advisor features
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiAdvisorController : ControllerBase
{
    private readonly IAiAdvisorService _aiAdvisorService;
    private readonly ILogger<AiAdvisorController> _logger;

    public AiAdvisorController(IAiAdvisorService aiAdvisorService, ILogger<AiAdvisorController> logger)
    {
        _aiAdvisorService = aiAdvisorService;
        _logger = logger;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get AI financial suggestions for the current user
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<AiSuggestionDto>>> GetSuggestions()
    {
        try
        {
            var userId = GetUserId();
            var suggestions = await _aiAdvisorService.GetSuggestionsAsync(userId);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI suggestions");
            return StatusCode(500, new { message = "An error occurred while retrieving suggestions" });
        }
    }

    /// <summary>
    /// Generate new AI financial suggestions based on spending patterns
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult> GenerateSuggestions([FromBody] GenerateAiSuggestionsDto? dto = null)
    {
        try
        {
            var userId = GetUserId();
            await _aiAdvisorService.GenerateSuggestionsAsync(userId, dto);
            return Ok(new { message = "AI suggestions generated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions");
            return StatusCode(500, new { message = "An error occurred while generating suggestions" });
        }
    }

    /// <summary>
    /// Chat with AI financial advisor
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _aiAdvisorService.ChatAsync(userId, request.Message);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return StatusCode(500, new { message = "An error occurred during chat" });
        }
    }

    /// <summary>
    /// Get daily AI insight for dashboard widget
    /// </summary>
    [HttpGet("daily-insight")]
    public async Task<ActionResult<AiInsightDto>> GetDailyInsight()
    {
        try
        {
            var userId = GetUserId();
            var insight = await _aiAdvisorService.GetDailyInsightAsync(userId);
            return Ok(insight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily insight");
            return StatusCode(500, new { message = "An error occurred while getting insight" });
        }
    }

    /// <summary>
    /// Get cashflow forecast
    /// </summary>
    [HttpGet("cashflow-forecast")]
    public async Task<ActionResult<AiCashflowForecastDto>> GetCashflowForecast()
    {
        try
        {
            var userId = GetUserId();
            var forecast = await _aiAdvisorService.GetCashflowForecastAsync(userId);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cashflow forecast");
            return StatusCode(500, new { message = "An error occurred while getting forecast" });
        }
    }

    /// <summary>
    /// Mark a suggestion as read
    /// </summary>
    [HttpPut("suggestions/{id}/read")]
    public async Task<ActionResult> MarkAsRead(long id)
    {
        try
        {
            var userId = GetUserId();
            await _aiAdvisorService.MarkSuggestionAsReadAsync(userId, id);
            return Ok(new { message = "Suggestion marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking suggestion as read");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
