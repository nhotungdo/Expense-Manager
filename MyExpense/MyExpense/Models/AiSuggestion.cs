using System;
using System.Collections.Generic;

namespace MyExpense.Models;

public partial class AiSuggestion
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Suggestion { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
