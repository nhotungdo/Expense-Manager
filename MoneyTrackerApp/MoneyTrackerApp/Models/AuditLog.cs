using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }

    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
