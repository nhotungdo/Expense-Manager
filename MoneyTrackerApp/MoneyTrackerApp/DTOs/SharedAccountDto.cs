using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for creating/updating shared account access
/// </summary>
public class ShareAccountDto
{
    [Required(ErrorMessage = "Account ID is required")]
    public long AccountId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public long UserId { get; set; }

    [Required(ErrorMessage = "Permission level is required")]
    [Range(0, 2, ErrorMessage = "Invalid permission level")]
    public int Permission { get; set; }
}

/// <summary>
/// DTO for returning shared account details
/// </summary>
public class SharedAccountResponseDto
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public long UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public int Permission { get; set; }

    public string PermissionDisplay { get; set; } = null!;

    public long SharedByUserId { get; set; }

    public string SharedByUserName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// DTO for returning list of shared accounts for a user
/// </summary>
public class SharedAccountListDto
{
    public long Id { get; set; }

    public string AccountName { get; set; } = null!;

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = null!;

    public int Permission { get; set; }

    public string PermissionDisplay { get; set; } = null!;

    public string SharedByUserName { get; set; } = null!;

    public string? Color { get; set; }

    public string? Icon { get; set; }
}
