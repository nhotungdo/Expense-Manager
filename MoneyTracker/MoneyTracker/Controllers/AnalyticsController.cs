using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAdvancedAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAdvancedAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("spending-analysis")]
        public async Task<IActionResult> GetSpendingAnalysis([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var analysis = await _analyticsService.GetSpendingAnalysisAsync(userId.Value, start, end);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spending analysis");
                return StatusCode(500, "Error getting spending analysis");
            }
        }

        [HttpGet("income-analysis")]
        public async Task<IActionResult> GetIncomeAnalysis([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var analysis = await _analyticsService.GetIncomeAnalysisAsync(userId.Value, start, end);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting income analysis");
                return StatusCode(500, "Error getting income analysis");
            }
        }

        [HttpGet("budget-analysis")]
        public async Task<IActionResult> GetBudgetAnalysis([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var analysis = await _analyticsService.GetBudgetAnalysisAsync(userId.Value, start, end);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting budget analysis");
                return StatusCode(500, "Error getting budget analysis");
            }
        }

        [HttpGet("financial-health")]
        public async Task<IActionResult> GetFinancialHealth()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var health = await _analyticsService.GetFinancialHealthAsync(userId.Value);
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial health");
                return StatusCode(500, "Error getting financial health");
            }
        }

        [HttpGet("trend-analysis")]
        public async Task<IActionResult> GetTrendAnalysis([FromQuery] int months = 12)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var trends = await _analyticsService.GetTrendAnalysisAsync(userId.Value, months);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend analysis");
                return StatusCode(500, "Error getting trend analysis");
            }
        }

        [HttpGet("category-insights")]
        public async Task<IActionResult> GetCategoryInsights([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var insights = await _analyticsService.GetCategoryInsightsAsync(userId.Value, start, end);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category insights");
                return StatusCode(500, "Error getting category insights");
            }
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> GetFinancialForecast([FromQuery] int months = 6)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var forecast = await _analyticsService.GetFinancialForecastAsync(userId.Value, months);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial forecast");
                return StatusCode(500, "Error getting financial forecast");
            }
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var startDate = DateTime.UtcNow.AddMonths(-1);
                var endDate = DateTime.UtcNow;

                var spendingAnalysis = await _analyticsService.GetSpendingAnalysisAsync(userId.Value, startDate, endDate);
                var incomeAnalysis = await _analyticsService.GetIncomeAnalysisAsync(userId.Value, startDate, endDate);
                var financialHealth = await _analyticsService.GetFinancialHealthAsync(userId.Value);
                var budgetAnalysis = await _analyticsService.GetBudgetAnalysisAsync(userId.Value, startDate, endDate);

                var summary = new
                {
                    Spending = new
                    {
                        Total = spendingAnalysis.TotalSpent,
                        Average = spendingAnalysis.AverageDailySpending,
                        TopCategory = spendingAnalysis.TopCategories.FirstOrDefault()?.CategoryName
                    },
                    Income = new
                    {
                        Total = incomeAnalysis.TotalIncome,
                        Average = incomeAnalysis.AverageDailyIncome,
                        TopCategory = incomeAnalysis.TopCategories.FirstOrDefault()?.CategoryName
                    },
                    Health = new
                    {
                        Score = financialHealth.HealthScore,
                        Status = financialHealth.HealthStatus,
                        SavingsRate = financialHealth.SavingsRate
                    },
                    Budget = new
                    {
                        Status = budgetAnalysis.BudgetStatus,
                        Utilization = budgetAnalysis.BudgetUtilization,
                        Remaining = budgetAnalysis.RemainingBudget
                    }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, "Error getting dashboard summary");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
