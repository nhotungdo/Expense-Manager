using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class GroupInvitation
{
    public long Id { get; set; }

    public long GroupId { get; set; }

    public long InviterId { get; set; }

    public string InviteEmail { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Status { get; set; } = null!; // Pending, Accepted, Rejected

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual GroupExpense Group { get; set; } = null!;

    public virtual User Inviter { get; set; } = null!;
}
