using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// API Controller for Reports and Analytics features
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get dashboard overview with charts and recent data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboard()
    {
        try
        {
            var userId = GetUserId();
            var dashboard = await _reportService.GetDashboardOverviewAsync(userId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard overview");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Generate cash flow report
    /// </summary>
    [HttpGet("cashflow")]
    public async Task<ActionResult<CashFlowReportDto>> GetCashFlowReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = GetUserId();
            var report = await _reportService.GenerateCashFlowReportAsync(userId, startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cash flow report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Generate monthly trend report
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<MonthlyTrendReportDto>> GetMonthlyTrends([FromQuery] int year)
    {
        try
        {
            var userId = GetUserId();
            var report = await _reportService.GenerateMonthlyTrendReportAsync(userId, year);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly trend report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Generate category breakdown report
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<CategoryBreakdownReportDto>> GetCategoryBreakdown([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = GetUserId();
            var report = await _reportService.GenerateCategoryBreakdownAsync(userId, startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating category breakdown report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Export report to file (PDF, Excel, CSV, JSON)
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult<string>> ExportReport([FromBody] GenerateReportDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var filePath = await _reportService.ExportReportAsync(userId, dto);
            return Ok(new { filePath, message = "Report exported successfully" });
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            return StatusCode(500, new { message = "An error occurred while exporting the report" });
        }
    }
}
