using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;

namespace MoneyTrackerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpGet("insights")]
        public async Task<ActionResult> GetInsights([FromQuery] string period = "week")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var result = await _analysisService.GetInsightsAsync(userId, period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting insights", details = ex.Message });
            }
        }

        [HttpGet("predictions")]
        public async Task<ActionResult> GetPredictions([FromQuery] string period = "week")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var result = await _analysisService.GetPredictionsAsync(userId, period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting predictions", details = ex.Message });
            }
        }

        [HttpGet("anomalies")]
        public async Task<ActionResult> GetAnomalies([FromQuery] string period = "week")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var result = await _analysisService.GetAnomaliesAsync(userId, period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting anomalies", details = ex.Message });
            }
        }

        [HttpGet("smart-recommendations")]
        public async Task<ActionResult> GetSmartRecommendations([FromQuery] string period = "week")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var result = await _analysisService.GetSmartRecommendationsAsync(userId, period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting recommendations", details = ex.Message });
            }
        }
    }
}
