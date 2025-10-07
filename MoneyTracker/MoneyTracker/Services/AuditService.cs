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
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit log created: User {UserId} performed {Action} on {EntityType} {EntityId}",
                    userId, action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for user {UserId}, action {Action}", userId, action);
            }
        }

        public async Task LogSystemEventAsync(string eventType, string description, object? data = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = null, // System event
                    Action = eventType,
                    Details = description,
                    EntityType = "SYSTEM",
                    EntityId = null,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    CreatedAt = DateTime.UtcNow
                };

                if (data != null)
                {
                    auditLog.Details += $" | Data: {JsonSerializer.Serialize(data)}";
                }

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("System audit log created: {EventType} - {Description}", eventType, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create system audit log for event {EventType}", eventType);
            }
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(long userId, int skip = 0, int take = 50)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetSystemAuditLogsAsync(int skip = 0, int take = 50)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == null)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        private string? GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                // Check for forwarded IP first (for load balancers/proxies)
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                // Check for real IP
                var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fallback to connection remote IP
                return httpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string? GetUserAgent()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}