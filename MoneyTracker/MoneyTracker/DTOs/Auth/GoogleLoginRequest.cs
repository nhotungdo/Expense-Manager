using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
