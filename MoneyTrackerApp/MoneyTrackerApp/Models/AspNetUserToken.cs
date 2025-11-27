using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class AspNetUserToken
{
    public long UserId { get; set; }

    public string LoginProvider { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public virtual User User { get; set; } = null!;
}
