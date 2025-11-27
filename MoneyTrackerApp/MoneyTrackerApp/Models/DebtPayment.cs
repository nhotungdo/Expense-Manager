using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class DebtPayment
{
    public long Id { get; set; }

    public long DebtId { get; set; }

    public long TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Note { get; set; }

    public virtual Debt Debt { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
