using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class SavingsTransaction
{
    public long Id { get; set; }

    public long SavingsGoalId { get; set; }

    public long TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Note { get; set; }

    public virtual SavingsGoal SavingsGoal { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
