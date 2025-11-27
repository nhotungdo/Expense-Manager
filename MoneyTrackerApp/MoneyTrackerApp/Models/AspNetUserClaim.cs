using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class AspNetUserClaim
{
    public int Id { get; set; }

    public long UserId { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public virtual User User { get; set; } = null!;
}
