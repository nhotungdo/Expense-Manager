using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class SavingsGoal
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateOnly? TargetDate { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SavingsTransaction> SavingsTransactions { get; set; } = new List<SavingsTransaction>();

    public virtual User User { get; set; } = null!;
}
