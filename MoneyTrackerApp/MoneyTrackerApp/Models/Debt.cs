using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Debt
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int DebtType { get; set; }

    public string Name { get; set; } = null!;

    public string? PersonName { get; set; }

    public decimal InitialAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal InterestRate { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DebtPayment> DebtPayments { get; set; } = new List<DebtPayment>();

    public virtual User User { get; set; } = null!;
}
