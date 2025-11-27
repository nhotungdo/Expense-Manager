using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class OnboardingStatus
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int CurrentStep { get; set; }

    public string? ProfileJson { get; set; }

    public string? IncomeJson { get; set; }

    public string? ExpensesJson { get; set; }

    public string? GoalsJson { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
