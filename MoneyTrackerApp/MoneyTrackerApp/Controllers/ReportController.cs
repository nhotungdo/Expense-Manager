using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache; // Caching service

    public ReportController(
        IReportService reportService, 
        IExportService exportService, 
        ILogger<ReportController> logger, 
        IWebHostEnvironment env,
        IMemoryCache cache)
    {
        _reportService = reportService;
        _exportService = exportService;
        _logger = logger;
        _env = env;
        _cache = cache;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get dashboard overview with charts and recent data
    /// Cached for 5 minutes to improve performance
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboard()
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0 && !_env.IsDevelopment()) return Unauthorized();

            // Cache Key based on User ID
            var cacheKey = $"dashboard_overview_{userId}";
            
            // Try get from cache
            if (_cache.TryGetValue(cacheKey, out DashboardOverviewDto cachedDashboard))
            {
                _logger.LogInformation("Returning cached dashboard for user {UserId}", userId);
                return Ok(cachedDashboard);
            }

            var dashboard = await _reportService.GetDashboardOverviewAsync(userId);
            
            // Set cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache for 5 mins
                .SetSlidingExpiration(TimeSpan.FromMinutes(2)) // Refresh text if accessed frequently
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, dashboard, cacheOptions);

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
            // Optional: Cache this if needed, but date ranges vary wildly so cache hit rate might be low.
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
    /// Cached for specific date ranges (e.g. current month) could be useful
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<CategoryBreakdownReportDto>> GetCategoryBreakdown([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = GetUserId();
            
            // Simple caching strategy for category breakdown
            var cacheKey = $"categories_{userId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
             if (_cache.TryGetValue(cacheKey, out CategoryBreakdownReportDto cachedReport))
            {
                 return Ok(cachedReport);
            }

            var report = await _reportService.GenerateCategoryBreakdownAsync(userId, startDate, endDate);
            
            _cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
            
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
    /// Export report based on criteria with role-based access control
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0 && !_env.IsDevelopment())
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Validate date range
            if (request.StartDate > request.EndDate)
            {
                return BadRequest(new { message = "Start date must be before end date" });
            }

            // Check date range limits (prevent excessive data export)
            var daysDifference = (request.EndDate - request.StartDate).TotalDays;
            if (daysDifference > 365)
            {
                return BadRequest(new { message = "Date range cannot exceed 365 days" });
            }

            // Role-based access control for advanced features
            var isPremiumUser = User.IsInRole("Premium") || User.IsInRole("Admin");
            
            // PDF export only for premium users
            if (request.FileFormat == 1 && !isPremiumUser)
            {
                return Forbid("PDF export is only available for Premium users");
            }

            // Advanced filtering only for premium users
            if ((request.AccountIds?.Any() == true || 
                 request.CategoryIds?.Any() == true || 
                 request.MinAmount.HasValue || 
                 request.MaxAmount.HasValue) && !isPremiumUser)
            {
                return Forbid("Advanced filtering is only available for Premium users");
            }

            byte[] content = Array.Empty<byte>();
            string fileName = "Report";
            string contentType = "application/octet-stream";

            // Determine file extension and content type
            switch (request.FileFormat)
            {
                case 1: // PDF
                    fileName += ".html"; // Using HTML for PDF fallback
                    contentType = "text/html";
                    break;
                case 2: // Excel
                    fileName += ".csv"; // Using CSV content for Excel for now
                    contentType = "application/vnd.ms-excel";
                    break;
                case 3: // CSV
                    fileName += ".csv";
                    contentType = "text/csv";
                    break;
                case 4: // JSON
                    fileName += ".json";
                    contentType = "application/json";
                    break;
                default:
                    return BadRequest(new { message = "Unsupported file format. Use 1=PDF, 2=Excel, 3=CSV, 4=JSON" });
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
                    return BadRequest(new { message = "Invalid report type. Use 1=CashFlow, 3=CategoryBreakdown, 4=MonthlyTrends" });
            }

            // Log export activity
            _logger.LogInformation(
                "User {UserId} exported {ReportType} report from {StartDate} to {EndDate} in {Format} format",
                userId, request.ReportType, request.StartDate, request.EndDate, request.FileFormat);

            // Set proper headers for file download
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return File(content, contentType, fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized export attempt");
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export parameters");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report for user {UserId}", GetUserId());
            return StatusCode(500, new { message = "An error occurred while exporting the report. Please try again later." });
        }
    }

    /// <summary>
    /// Get AI-powered financial analysis (Premium feature)
    /// </summary>
    [HttpGet("ai-analysis")]
    [Authorize(Roles = "Premium,Admin")]
    public async Task<ActionResult> GetAiAnalysis()
    {
        try
        {
            var userId = GetUserId();
            
            // This would integrate with an AI service
            // For now, return mock data structure
            var analysis = new
            {
                insights = new[]
                {
                    new { type = "warning", icon = "fa-exclamation-triangle", title = "Chi tiêu vượt mức", description = "Chi tiêu tháng này cao hơn 23% so với trung bình", color = "red" },
                    new { type = "success", icon = "fa-check-circle", title = "Tiết kiệm tốt", description = "Bạn đã tiết kiệm được 15% thu nhập", color = "green" }
                },
                spendingPattern = new
                {
                    labels = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" },
                    data = new[] { 1200000, 850000, 1500000, 950000, 2100000, 1800000, 2500000 }
                },
                recommendations = new[]
                {
                    new { icon = "fa-piggy-bank", title = "Tăng tiết kiệm", description = "Đặt mục tiêu tiết kiệm 20% thu nhập", priority = "high" }
                }
            };

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI analysis");
            return StatusCode(500, new { message = "An error occurred while generating AI analysis" });
        }
    }

    /// <summary>
    /// Get report access permissions for current user
    /// </summary>
    [HttpGet("permissions")]
    public ActionResult GetReportPermissions()
    {
        var permissions = new
        {
            canExportPdf = User.IsInRole("Premium") || User.IsInRole("Admin"),
            canExportExcel = true,
            canExportCsv = true,
            canUseAdvancedFilters = User.IsInRole("Premium") || User.IsInRole("Admin"),
            canAccessAiInsights = User.IsInRole("Premium") || User.IsInRole("Admin"),
            maxDateRangeDays = User.IsInRole("Premium") || User.IsInRole("Admin") ? 365 : 90,
            canScheduleReports = User.IsInRole("Premium") || User.IsInRole("Admin")
        };

        return Ok(permissions);
    }
}
