namespace MoneyTrackerApp.Enums;

/// <summary>
/// Onboarding step enumeration
/// </summary>
public enum OnboardingStep
{
    NotStarted = 0,
    Welcome = 1,
    BasicSettings = 2,
    CreateWallet = 3,
    SetupCategories = 4,
    SavingsGoal = 5,
    Completed = 6
}

/// <summary>
/// Category template enumeration
/// </summary>
public enum CategoryTemplate
{
    Student,
    Family,
    Business,
    Freelancer,
    Minimal
}

/// <summary>
/// Supported currencies
/// </summary>
public enum SupportedCurrency
{
    VND,
    USD,
    EUR,
    GBP,
    JPY
}

/// <summary>
/// Supported languages
/// </summary>
public enum SupportedLanguage
{
    Vietnamese,
    English
}
