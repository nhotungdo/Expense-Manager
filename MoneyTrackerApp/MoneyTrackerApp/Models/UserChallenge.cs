using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class UserChallenge
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long ChallengeId { get; set; }

    public string Status { get; set; } = null!; // Active, Completed, Failed

    public decimal Progress { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Challenge Challenge { get; set; } = null!;
}
