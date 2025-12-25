using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class KidTask
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    public long ChildId { get; set; } // User Id of the child

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal RewardAmount { get; set; }

    public string Status { get; set; } = null!; // Pending, Completed, Approved, Rejected

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User Parent { get; set; } = null!;
    
    public virtual User Child { get; set; } = null!;
}
