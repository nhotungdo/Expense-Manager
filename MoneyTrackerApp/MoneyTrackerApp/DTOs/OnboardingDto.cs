using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs;

/// <summary>
/// DTO for onboarding status
/// </summary>
public class OnboardingStatusDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int CurrentStep { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public OnboardingProfileDto? Profile { get; set; }
    public OnboardingWalletDto? Wallet { get; set; }
    public OnboardingCategorySetupDto? CategorySetup { get; set; }
    public OnboardingSavingsGoalDto? SavingsGoal { get; set; }
}

/// <summary>
/// DTO for basic settings (Step 1)
/// </summary>
public class OnboardingProfileDto
{
    [Required(ErrorMessage = "Currency is required")]
    public string Currency { get; set; } = "VND";

    [Required(ErrorMessage = "Language is required")]
    public string Language { get; set; } = "vi";

    public string? Timezone { get; set; }
    public string? Theme { get; set; } = "light";
}

/// <summary>
/// DTO for first wallet creation (Step 2)
/// </summary>
public class OnboardingWalletDto
{
    [Required(ErrorMessage = "Wallet name is required")]
    [StringLength(100, ErrorMessage = "Wallet name cannot exceed 100 characters")]
    public string Name { get; set; } = "Cash Wallet";

    [Required(ErrorMessage = "Wallet type is required")]
    public int AccountType { get; set; } = 0; // Cash

    [Required(ErrorMessage = "Initial balance is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Initial balance must be positive")]
    public decimal InitialBalance { get; set; } = 0;

    public string? Icon { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// DTO for category setup (Step 3)
/// </summary>
public class OnboardingCategorySetupDto
{
    [Required(ErrorMessage = "Template is required")]
    public string Template { get; set; } = "Student";

    public List<CategoryPreviewDto>? CustomCategories { get; set; }
}

/// <summary>
/// DTO for category preview
/// </summary>
public class CategoryPreviewDto
{
    public string Name { get; set; } = null!;
    public int Type { get; set; } // 0: Expense, 1: Income
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for savings goal (Step 4 - Optional)
/// </summary>
public class OnboardingSavingsGoalDto
{
    [StringLength(100, ErrorMessage = "Goal name cannot exceed 100 characters")]
    public string? Name { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
    public decimal? TargetAmount { get; set; }

    public DateOnly? TargetDate { get; set; }

    public string? Icon { get; set; }
    public string? Color { get; set; }

    // Calculated field
    public decimal? MonthlyAmount { get; set; }
}

/// <summary>
/// DTO for completing onboarding
/// </summary>
public class CompleteOnboardingDto
{
    public OnboardingProfileDto Profile { get; set; } = null!;
    public OnboardingWalletDto Wallet { get; set; } = null!;
    public OnboardingCategorySetupDto CategorySetup { get; set; } = null!;
    public OnboardingSavingsGoalDto? SavingsGoal { get; set; }
}

/// <summary>
/// DTO for updating onboarding step
/// </summary>
public class UpdateOnboardingStepDto
{
    [Required]
    public int Step { get; set; }

    public string? StepData { get; set; }
}

/// <summary>
/// DTO for login/register request
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// DTO for register request
/// </summary>
public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

/// <summary>
/// DTO for Google Sign-In
/// </summary>
public class GoogleSignInDto
{
    [Required]
    public string IdToken { get; set; } = null!;
}

/// <summary>
/// DTO for authentication response
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public UserInfoDto? User { get; set; }
}

/// <summary>
/// DTO for user information
/// </summary>
public class UserInfoDto
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool OnboardingCompleted { get; set; }
}
