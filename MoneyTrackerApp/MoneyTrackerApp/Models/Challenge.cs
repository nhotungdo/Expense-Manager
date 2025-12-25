using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class Challenge
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Type { get; set; } = null!; // NoSpend, SavingsTarget

    public long? TargetCategoryId { get; set; }

    public decimal? TargetAmount { get; set; }

    public int DurationDays { get; set; }

    public int RewardPoints { get; set; }

    public string? BadgeIcon { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category? TargetCategory { get; set; }

    public virtual ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
}
