using System.Collections.Generic;

namespace MoneyTracker.Models;

public enum BudgetPeriod
{
    Weekly = 1,
    Monthly = 2,
    Yearly = 3
}

public partial class Budget
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual User User { get; set; } = null!;
}
