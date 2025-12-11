using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers.Admin;

[ApiController]
[Route("api/admin/ai")]
[Authorize(Roles = "Admin")]
public class AdminAiController : ControllerBase
{
    private readonly IAdminAiService _adminAiService;
    private readonly ILogger<AdminAiController> _logger;

    public AdminAiController(
        IAdminAiService adminAiService,
        ILogger<AdminAiController> logger)
    {
        _adminAiService = adminAiService;
        _logger = logger;
    }

    /// <summary>
    /// Get churn prediction for users at risk
    /// </summary>
    [HttpGet("churn-prediction")]
    public async Task<ActionResult<List<ChurnPredictionDto>>> GetChurnPredictions()
    {
        try
        {
            var predictions = await _adminAiService.GetChurnPredictionsAsync();
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn predictions");
            return StatusCode(500, new { message = "An error occurred while getting churn predictions" });
        }
    }

    /// <summary>
    /// Detect fraud patterns and suspicious activities
    /// </summary>
    [HttpGet("fraud-detection")]
    public async Task<ActionResult<List<FraudDetectionDto>>> DetectFraud()
    {
        try
        {
            var fraudAlerts = await _adminAiService.DetectFraudAsync();
            return Ok(fraudAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting fraud");
            return StatusCode(500, new { message = "An error occurred while detecting fraud" });
        }
    }

    /// <summary>
    /// Process natural language query for data analysis
    /// </summary>
    [HttpPost("query")]
    public async Task<ActionResult<NaturalLanguageResponseDto>> ProcessQuery([FromBody] NaturalLanguageQueryDto request)
    {
        try
        {
            var response = await _adminAiService.ProcessNaturalLanguageQueryAsync(request.Query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query");
            return StatusCode(500, new { message = "An error occurred while processing query" });
        }
    }
}

