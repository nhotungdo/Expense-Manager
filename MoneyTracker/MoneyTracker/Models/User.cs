using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class User
{
    public long Id { get; set; }

    public string GoogleId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? PictureUrl { get; set; }

    public string Role { get; set; } = null!;

    public bool Enabled { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
}
