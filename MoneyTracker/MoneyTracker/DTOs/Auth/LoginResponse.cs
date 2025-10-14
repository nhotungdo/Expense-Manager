namespace MoneyTracker.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public bool IsNewUser { get; set; }
}

public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public bool OnboardingCompleted { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string Language { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
}
