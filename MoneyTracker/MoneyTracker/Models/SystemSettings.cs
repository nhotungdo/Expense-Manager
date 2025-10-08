using System;
using System.Collections.Generic;

namespace MoneyTracker.Models;

public partial class SystemSettings
{
    public long Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public string SettingType { get; set; } = null!; // "string", "number", "boolean", "json"

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
