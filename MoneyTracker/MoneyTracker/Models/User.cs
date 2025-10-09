using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MoneyTracker.Models;

public partial class User : IdentityUser<long>
{
    public string GoogleId { get; set; } = null!;

    public string? FullName { get; set; }

    public string? PictureUrl { get; set; }

    public string Role { get; set; } = null!;

    public bool Enabled { get; set; } = true;

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string Language { get; set; } = "vi";

    public string DefaultCurrency { get; set; } = "VND";

    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    public string Theme { get; set; } = "light";

    public bool EmailNotifications { get; set; } = true;

    public bool PushNotifications { get; set; } = true;

    public virtual ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
}
