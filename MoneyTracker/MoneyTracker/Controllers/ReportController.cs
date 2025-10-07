using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportExportService _reportExportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportExportService reportExportService, ILogger<ReportController> logger)
        {
            _reportExportService = reportExportService;
            _logger = logger;
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string reportType = "monthly")
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var pdfBytes = await _reportExportService.ExportToPdfAsync(userId.Value, start, end, reportType);

                var fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PDF report for user {UserId}", userId);
                return StatusCode(500, "Error generating PDF report");
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string reportType = "monthly")
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var excelBytes = await _reportExportService.ExportToExcelAsync(userId.Value, start, end, reportType);

                var fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.xlsx";

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Excel report for user {UserId}", userId);
                return StatusCode(500, "Error generating Excel report");
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string reportType = "monthly")
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var csvBytes = await _reportExportService.ExportToCsvAsync(userId.Value, start, end, reportType);

                var fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV report for user {UserId}", userId);
                return StatusCode(500, "Error generating CSV report");
            }
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendReportByEmail(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string reportType = "monthly",
            [FromQuery] string format = "pdf")
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                byte[] reportBytes;
                string fileName;
                string mimeType;

                switch (format.ToLower())
                {
                    case "excel":
                        reportBytes = await _reportExportService.ExportToExcelAsync(userId.Value, start, end, reportType);
                        fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.xlsx";
                        mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case "csv":
                        reportBytes = await _reportExportService.ExportToCsvAsync(userId.Value, start, end, reportType);
                        fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.csv";
                        mimeType = "text/csv";
                        break;
                    default:
                        reportBytes = await _reportExportService.ExportToPdfAsync(userId.Value, start, end, reportType);
                        fileName = $"bao-cao-tai-chinh-{start:yyyy-MM}-{end:yyyy-MM}.pdf";
                        mimeType = "application/pdf";
                        break;
                }

                // Here you would typically send the email with the attachment
                // For now, we'll return the file for download
                return File(reportBytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending report by email for user {UserId}", userId);
                return StatusCode(500, "Error sending report by email");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}