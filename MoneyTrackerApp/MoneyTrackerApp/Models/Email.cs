using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Email
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}
