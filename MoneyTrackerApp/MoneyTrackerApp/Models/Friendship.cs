using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Friendship
{
    public long Id { get; set; }

    public long RequesterId { get; set; }

    public long ReceiverId { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Requester { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;
}
