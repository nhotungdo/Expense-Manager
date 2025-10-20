using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class ScheduledTransaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long AccountId { get; set; }

    public long? CategoryId { get; set; }

    public int TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Frequency { get; set; } = null!;

    public int Interval { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly NextRunDate { get; set; }

    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category? Category { get; set; }

    public virtual User User { get; set; } = null!;
}
