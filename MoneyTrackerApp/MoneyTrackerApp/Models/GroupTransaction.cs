using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class GroupTransaction
{
    public long Id { get; set; }

    public long GroupId { get; set; }

    public long PaidByUserId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public string? Category { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GroupExpense Group { get; set; } = null!;

    public virtual ICollection<GroupTransactionSplit> GroupTransactionSplits { get; set; } = new List<GroupTransactionSplit>();

    public virtual User PaidByUser { get; set; } = null!;
}
