using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public interface IAuditService
    {
        Task LogUserActionAsync(long userId, string action, string details, string? entityType = null, long? entityId = null);
        Task LogSystemEventAsync(string eventType, string description, object? data = null);
        Task<List<AuditLog>> GetUserAuditLogsAsync(long userId, int skip = 0, int take = 50);
        Task<List<AuditLog>> GetSystemAuditLogsAsync(int skip = 0, int take = 50);
    }
}
