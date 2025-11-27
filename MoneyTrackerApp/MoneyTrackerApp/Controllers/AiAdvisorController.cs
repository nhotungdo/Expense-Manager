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
}
