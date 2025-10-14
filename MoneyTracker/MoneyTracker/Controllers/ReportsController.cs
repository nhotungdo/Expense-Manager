using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<dynamic>> GetSummary([FromQuery] string period = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var summary = await _reportService.GetSummaryReportAsync(userId.Value, period);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary report");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetCategoryBreakdown([FromQuery] string period = "month")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var breakdown = await _reportService.GetCategoryBreakdownReportAsync(userId.Value, period);
            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category breakdown report");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("income-expense-trend")]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetIncomeExpenseTrend([FromQuery] string period = "year")
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var trend = await _reportService.GetIncomeExpenseTrendReportAsync(userId.Value, period);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting income expense trend report");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportTransactions([FromQuery] string format = "excel", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var fileBytes = await _reportService.ExportTransactionsAsync(userId.Value, format, startDate.Value, endDate.Value);
            var contentType = format.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream"
            };

            var fileName = $"transactions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions");
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
