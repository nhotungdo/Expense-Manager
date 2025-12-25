using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class ShoppingItem
{
    public long Id { get; set; }

    public long ShoppingListId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? EstimatedPrice { get; set; }

    public bool IsPurchased { get; set; }

    public virtual ShoppingList ShoppingList { get; set; } = null!;
}
