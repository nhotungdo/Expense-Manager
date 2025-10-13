using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class Budget
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? CategoryId { get; set; }

    public decimal BudgetAmount { get; set; }

    public decimal SpentAmount { get; set; }

    public string? Currency { get; set; }

    public string PeriodType { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual User User { get; set; } = null!;
}
