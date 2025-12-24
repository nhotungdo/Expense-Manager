using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.Models;

public partial class UserSession
{
    public Guid Id { get; set; }

    public long UserId { get; set; }

    public string? DeviceName { get; set; }
    
    public string? DeviceType { get; set; }

    public string? OperatingSystem { get; set; }

    public string? Browser { get; set; }

    public string? IpAddress { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    public bool IsActive { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual User User { get; set; } = null!;
}
