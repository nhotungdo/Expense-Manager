using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class Expense
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? Note { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual User User { get; set; } = null!;
}
