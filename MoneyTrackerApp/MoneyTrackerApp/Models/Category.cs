using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Category
{
    public long Id { get; set; }

    public long? ParentCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public int Type { get; set; }

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public long? UserId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<ScheduledTransaction> ScheduledTransactions { get; set; } = new List<ScheduledTransaction>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
