using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class GroupExpense
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public long CreatedByUserId { get; set; }

    public bool IsPublic { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<GroupTransaction> GroupTransactions { get; set; } = new List<GroupTransaction>();

    public virtual ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();
}
