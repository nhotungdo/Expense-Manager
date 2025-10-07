using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;
        private readonly IValidationService _validationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IPerformanceService performanceService,
            IValidationService validationService,
            IAuditService auditService,
            ILogger<AdminController> logger)
        {
            _performanceService = performanceService;
            _validationService = validationService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("database-stats")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            try
            {
                var stats = await _performanceService.GetDatabaseStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database stats");
                return StatusCode(500, "Error retrieving database statistics");
            }
        }

        [HttpPost("optimize-database")]
        public async Task<IActionResult> OptimizeDatabase()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _performanceService.OptimizeDatabaseAsync();
                await _auditService.LogUserActionAsync(userId.Value, "DATABASE_OPTIMIZATION", "Database optimization performed", "System");

                return Ok(new { message = "Database optimization completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing database");
                return StatusCode(500, "Error optimizing database");
            }
        }

        [HttpPost("cleanup-data")]
        public async Task<IActionResult> CleanupData()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _performanceService.CleanupOldDataAsync();
                await _auditService.LogUserActionAsync(userId.Value, "DATA_CLEANUP", "Old data cleanup performed", "System");

                return Ok(new { message = "Data cleanup completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up data");
                return StatusCode(500, "Error cleaning up data");
            }
        }

        [HttpPost("rebuild-indexes")]
        public async Task<IActionResult> RebuildIndexes()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _performanceService.RebuildIndexesAsync();
                await _auditService.LogUserActionAsync(userId.Value, "INDEX_REBUILD", "Database indexes rebuilt", "System");

                return Ok(new { message = "Index rebuild completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding indexes");
                return StatusCode(500, "Error rebuilding indexes");
            }
        }

        [HttpPost("analyze-performance")]
        public async Task<IActionResult> AnalyzePerformance()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _performanceService.AnalyzeQueryPerformanceAsync();
                await _auditService.LogUserActionAsync(userId.Value, "PERFORMANCE_ANALYSIS", "Query performance analysis performed", "System");

                return Ok(new { message = "Performance analysis completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing performance");
                return StatusCode(500, "Error analyzing performance");
            }
        }

        [HttpGet("system-health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var stats = await _performanceService.GetDatabaseStatsAsync();

                var health = new
                {
                    Status = "Healthy",
                    DatabaseSize = FormatBytes(stats.DatabaseSizeBytes),
                    TotalUsers = stats.TotalUsers,
                    TotalTransactions = stats.TotalExpenses + stats.TotalIncomes,
                    SlowQueries = stats.SlowQueries.Count,
                    Recommendations = stats.Recommendations,
                    LastChecked = DateTime.UtcNow
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return StatusCode(500, "Error retrieving system health");
            }
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var systemLogs = await _auditService.GetSystemAuditLogsAsync(skip, take);
                return Ok(systemLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return StatusCode(500, "Error retrieving audit logs");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}