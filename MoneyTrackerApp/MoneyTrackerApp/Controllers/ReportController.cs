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
        private readonly IExportService _exportService;
        private readonly ILogger<ReportController> _logger;
        private readonly IWebHostEnvironment _env;

        public ReportController(IReportService reportService, IExportService exportService, ILogger<ReportController> logger, IWebHostEnvironment env)
        {
            _reportService = reportService;
            _exportService = exportService;
            _logger = logger;
            _env = env;
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
    [AllowAnonymous]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboard()
    {
        try
        {
            // Allow anonymous access in Development for Dev preview/testing
            if (!_env.IsDevelopment() && (User?.Identity?.IsAuthenticated != true))
            {
                return Unauthorized();
            }

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
    /// Export transactions to Excel
    /// </summary>
    [HttpGet("export/transactions/excel")]
    public async Task<IActionResult> ExportTransactionsExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? accountIds)
    {
        try
        {
            var userId = GetUserId();
            List<long>? accIds = null;
            if (!string.IsNullOrEmpty(accountIds))
            {
                accIds = accountIds.Split(',').Select(long.Parse).ToList();
            }

            var content = await _exportService.ExportTransactionsToExcelAsync(userId, startDate, endDate, accIds);
            var fileName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions to Excel");
            return StatusCode(500, new { message = "An error occurred while exporting" });
        }
    }

    /// <summary>
    /// Export transactions to PDF
    /// </summary>
    [HttpGet("export/transactions/pdf")]
    public async Task<IActionResult> ExportTransactionsPdf([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? accountIds)
    {
        try
        {
            var userId = GetUserId();
            List<long>? accIds = null;
            if (!string.IsNullOrEmpty(accountIds))
            {
                accIds = accountIds.Split(',').Select(long.Parse).ToList();
            }

            var content = await _exportService.ExportTransactionsToPdfAsync(userId, startDate, endDate, accIds);
            var fileName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(content, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions to PDF");
            return StatusCode(500, new { message = "An error occurred while exporting" });
        }
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    [HttpGet("export/transactions/csv")]
    public async Task<IActionResult> ExportTransactionsCsv([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? accountIds)
    {
        try
        {
            var userId = GetUserId();
            List<long>? accIds = null;
            if (!string.IsNullOrEmpty(accountIds))
            {
                accIds = accountIds.Split(',').Select(long.Parse).ToList();
            }

            var content = await _exportService.ExportTransactionsToCsvAsync(userId, startDate, endDate, accIds);
            var fileName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(content, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transactions to CSV");
            return StatusCode(500, new { message = "An error occurred while exporting" });
        }
    }

    /// <summary>
    /// Export cash flow report to Excel
    /// </summary>
    [HttpGet("export/cashflow/excel")]
    public async Task<IActionResult> ExportCashFlowExcel([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var userId = GetUserId();
            var content = await _exportService.ExportCashFlowReportToExcelAsync(userId, year, month);
            var fileName = $"CashFlow_{month}_{year}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting cash flow report");
            return StatusCode(500, new { message = "An error occurred while exporting" });
        }
    }

    /// <summary>
    /// Export category report to Excel
    /// </summary>
    [HttpGet("export/categories/excel")]
    public async Task<IActionResult> ExportCategoriesExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var userId = GetUserId();
            var content = await _exportService.ExportCategoryReportToExcelAsync(userId, startDate, endDate);
            var fileName = $"Categories_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting category report");
            return StatusCode(500, new { message = "An error occurred while exporting" });
        }
    }
    /// <summary>
    /// Export report based on criteria
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            byte[] content = Array.Empty<byte>();
            string fileName = "Report";
            string contentType = "application/octet-stream";

            // Determine file extension and content type
            if (request.FileFormat == 1) // PDF
            {
                fileName += ".html"; // Using HTML for PDF fallback
                contentType = "text/html";
            }
            else if (request.FileFormat == 2) // Excel
            {
                fileName += ".csv"; // Using CSV content for Excel for now
                contentType = "application/vnd.ms-excel";
            }
            else if (request.FileFormat == 3) // CSV
            {
                fileName += ".csv";
                contentType = "text/csv";
            }
            else if (request.FileFormat == 4) // JSON
            {
                fileName += ".json";
                contentType = "application/json";
            }
            else
            {
                return BadRequest(new { message = "Unsupported file format." });
            }

            // Generate content based on report type
            switch (request.ReportType)
            {
                case 1: // Cash Flow
                    if (request.FileFormat == 1)
                        content = await _exportService.ExportCashFlowReportToPdfAsync(userId, request.StartDate, request.EndDate);
                    else if (request.FileFormat == 4)
                        content = await _exportService.ExportCashFlowReportToJsonAsync(userId, request.StartDate, request.EndDate);
                    else
                        content = await _exportService.ExportCashFlowReportToExcelAsync(userId, request.StartDate, request.EndDate);
                    
                    fileName = $"CashFlow_{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}{Path.GetExtension(fileName)}";
                    break;

                case 3: // Category Breakdown
                    if (request.FileFormat == 1)
                        content = await _exportService.ExportCategoryReportToPdfAsync(userId, request.StartDate, request.EndDate);
                    else if (request.FileFormat == 4)
                        content = await _exportService.ExportCategoryReportToJsonAsync(userId, request.StartDate, request.EndDate);
                    else
                        content = await _exportService.ExportCategoryReportToExcelAsync(userId, request.StartDate, request.EndDate);
                        
                    fileName = $"CategoryBreakdown_{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}{Path.GetExtension(fileName)}";
                    break;

                case 4: // Monthly Trends
                    if (request.FileFormat == 1)
                        content = await _exportService.ExportMonthlyTrendsToPdfAsync(userId, request.StartDate.Year);
                    else if (request.FileFormat == 4)
                        content = await _exportService.ExportMonthlyTrendsToJsonAsync(userId, request.StartDate.Year);
                    else
                        content = await _exportService.ExportMonthlyTrendsToExcelAsync(userId, request.StartDate.Year);
                        
                    fileName = $"MonthlyTrends_{request.StartDate.Year}{Path.GetExtension(fileName)}";
                    break;

                default:
                    return BadRequest(new { message = "Invalid report type." });
            }

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            return StatusCode(500, new { message = "An error occurred while exporting the report." });
        }
    }
}
