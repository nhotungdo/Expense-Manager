namespace MoneyTracker.Services
{
    public interface IAuditService
    {
        Task LogUserActionAsync(long userId, string action, string details, string? entityType = null, long? entityId = null);
        Task LogSystemEventAsync(string eventType, string description, object? data = null);
        Task<List<AuditLog>> GetUserAuditLogsAsync(long userId, int skip = 0, int take = 50);
        Task<List<AuditLog>> GetSystemAuditLogsAsync(int skip = 0, int take = 50);
    }

    public class AuditLog
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public long? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
