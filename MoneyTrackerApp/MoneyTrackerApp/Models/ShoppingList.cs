using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class ShoppingList
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<ShoppingItem> ShoppingItems { get; set; } = new List<ShoppingItem>();
}
