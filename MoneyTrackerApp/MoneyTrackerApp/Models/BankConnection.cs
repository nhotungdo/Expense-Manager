using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class BankConnection
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long AccountId { get; set; }

    public string Provider { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public string? ItemId { get; set; }

    public DateTime? LastSync { get; set; }

    public string? SyncStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
