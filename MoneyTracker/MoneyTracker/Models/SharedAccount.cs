using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class SharedAccount
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long UserId { get; set; }

    public int Permission { get; set; }

    public long SharedByUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual User SharedByUser { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
