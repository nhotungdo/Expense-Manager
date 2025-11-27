using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class GroupMember
{
    public long Id { get; set; }

    public long GroupId { get; set; }

    public long UserId { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? JoinedAt { get; set; }

    public virtual GroupExpense Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
