using System;
using System.Collections.Generic;

namespace MyExpense.Models;

public partial class Category
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public long? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual User? User { get; set; }
}
