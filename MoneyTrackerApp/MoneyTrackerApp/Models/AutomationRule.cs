using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class AutomationRule
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public string TriggerType { get; set; } = null!; // TransactionCreated, BalanceThreshold, etc.

    public string ConditionJson { get; set; } = null!; // JSON for flexibility

    public string ActionType { get; set; } = null!; // Transfer, Notify, Block

    public string ActionJson { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastExecutedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
