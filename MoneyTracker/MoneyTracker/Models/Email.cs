using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class Email
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

