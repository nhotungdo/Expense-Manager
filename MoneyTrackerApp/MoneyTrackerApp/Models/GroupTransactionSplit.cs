using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class GroupTransactionSplit
{
    public long Id { get; set; }

    public long GroupTransactionId { get; set; }

    public long UserId { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual GroupTransaction GroupTransaction { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
