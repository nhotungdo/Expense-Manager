using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/user-ai")]
[Authorize]
public class UserAiController : ControllerBase
{
    private readonly IUserAiService _userAiService;
    private readonly ILogger<UserAiController> _logger;

    public UserAiController(
        IUserAiService userAiService,
        ILogger<UserAiController> logger)
    {
        _userAiService = userAiService;
        _logger = logger;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get smart plan recommendation based on user behavior
    /// </summary>
    [HttpGet("plan-recommendation")]
    public async Task<ActionResult<PlanRecommendationDto>> GetPlanRecommendation()
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var recommendation = await _userAiService.GetPlanRecommendationAsync(userId);
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan recommendation");
            return StatusCode(500, new { message = "An error occurred while getting recommendation" });
        }
    }

    /// <summary>
    /// Explain why bill amount changed
    /// </summary>
    [HttpPost("explain-bill")]
    public async Task<ActionResult<BillExplanationDto>> ExplainBill([FromBody] decimal currentAmount)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var explanation = await _userAiService.ExplainBillAsync(userId, currentAmount);
            return Ok(explanation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining bill");
            return StatusCode(500, new { message = "An error occurred while explaining bill" });
        }
    }

    /// <summary>
    /// Get spending forecast for next month
    /// </summary>
    [HttpGet("spending-forecast")]
    public async Task<ActionResult<SpendingForecastDto>> GetSpendingForecast()
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var forecast = await _userAiService.GetSpendingForecastAsync(userId);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spending forecast");
            return StatusCode(500, new { message = "An error occurred while getting forecast" });
        }
    }

    /// <summary>
    /// Search transactions using natural language
    /// </summary>
    [HttpPost("search-transactions")]
    public async Task<ActionResult<TransactionSearchResultDto>> SearchTransactions([FromBody] TransactionSearchRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var result = await _userAiService.SearchTransactionsAsync(userId, request.Query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching transactions");
            return StatusCode(500, new { message = "An error occurred while searching transactions" });
        }
    }

    /// <summary>
    /// Answer transaction-related questions
    /// </summary>
    [HttpPost("answer-question")]
    public async Task<ActionResult<string>> AnswerQuestion([FromBody] string question)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var answer = await _userAiService.AnswerTransactionQuestionAsync(userId, question);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering question");
            return StatusCode(500, new { message = "An error occurred while processing question" });
        }
    }
}

