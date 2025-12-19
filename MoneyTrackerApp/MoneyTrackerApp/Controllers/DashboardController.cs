using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IReportService _reportService;

        public DashboardController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardOverviewDto>> GetDashboardData()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var dashboardData = await _reportService.GetDashboardOverviewAsync(userId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard data", details = ex.Message });
            }
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<DashboardAnalyticsDto>> GetAnalyticsData([FromQuery] int days = 30)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var analyticsData = await _reportService.GetDashboardAnalyticsAsync(userId, days);
                return Ok(analyticsData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving analytics data", details = ex.Message });
            }
        }

        [HttpGet("personal-wallet")]
        public async Task<ActionResult> GetPersonalWalletSummary()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var summary = await _reportService.GetPersonalWalletSummaryAsync(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving wallet summary", details = ex.Message });
            }
        }

        [HttpGet("expense-breakdown")]
        public async Task<ActionResult> GetExpenseBreakdown([FromQuery] string period = "month")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var breakdown = await _reportService.GetExpenseBreakdownAsync(userId, period);
                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving expense breakdown", details = ex.Message });
            }
        }

        [HttpGet("income-breakdown")]
        public async Task<ActionResult> GetIncomeBreakdown([FromQuery] string period = "month")
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                var breakdown = await _reportService.GetIncomeBreakdownAsync(userId, period);
                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving income breakdown", details = ex.Message });
            }
        }
    }
}
