using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class AspNetUserLogin
{
    public string LoginProvider { get; set; } = null!;

    public string ProviderKey { get; set; } = null!;

    public string? ProviderDisplayName { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
