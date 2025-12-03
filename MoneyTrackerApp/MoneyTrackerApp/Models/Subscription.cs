using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

/// <summary>
/// User subscription model
/// </summary>
public partial class Subscription
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int PackageId { get; set; }

    public int Status { get; set; } // 0 = Pending, 1 = Active, 2 = Expired, 3 = Cancelled, 4 = Suspended

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public bool AutoRenew { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ServicePackage Package { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
