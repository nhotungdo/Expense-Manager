using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using System.Text.Json;

namespace MoneyTracker.Services
{
    public class AuditService : IAuditService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<AuditService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ExpenseManagerContext context, ILogger<AuditService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogUserActionAsync(long userId, string action, string details, string? entityType = null, long? entityId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                // For now, we'll log to Serilog since we don't have audit_logs table yet
                _logger.LogInformation("User Action: {@AuditLog}", auditLog);

                // TODO: Save to database when audit_logs table is created
                // _context.AuditLogs.Add(auditLog);
                // await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user action");
            }
        }

        public async Task LogSystemEventAsync(string eventType, string description, object? data = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = null,
                    Action = eventType,
                    Details = description,
                    EntityType = "System",
                    CreatedAt = DateTime.UtcNow
                };

                if (data != null)
                {
                    auditLog.Details += $" Data: {JsonSerializer.Serialize(data)}";
                }

                _logger.LogInformation("System Event: {@AuditLog}", auditLog);

                // TODO: Save to database when audit_logs table is created
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log system event");
            }
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(long userId, int skip = 0, int take = 50)
        {
            // TODO: Implement when audit_logs table is created
            // For now, return empty list
            await Task.CompletedTask;
            return new List<AuditLog>();
        }

        public async Task<List<AuditLog>> GetSystemAuditLogsAsync(int skip = 0, int take = 50)
        {
            // TODO: Implement when audit_logs table is created
            // For now, return empty list
            await Task.CompletedTask;
            return new List<AuditLog>();
        }
    }
}
