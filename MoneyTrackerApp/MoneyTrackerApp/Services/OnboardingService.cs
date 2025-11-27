using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Enums;
using MoneyTrackerApp.Models;
using System.Text.Json;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing user onboarding flow
/// </summary>
public class OnboardingService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(ExpenseManagerContext context, ILogger<OnboardingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get onboarding status for a user
    /// </summary>
    public async Task<OnboardingStatusDto?> GetOnboardingStatusAsync(long userId)
    {
        var status = await _context.OnboardingStatuses
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (status == null)
        {
            return null;
        }

        return new OnboardingStatusDto
        {
            Id = status.Id,
            UserId = status.UserId,
            CurrentStep = status.CurrentStep,
            IsCompleted = status.IsCompleted,
            StartedAt = status.StartedAt,
            UpdatedAt = status.UpdatedAt,
            CompletedAt = status.CompletedAt,
            Profile = string.IsNullOrEmpty(status.ProfileJson) 
                ? null 
                : JsonSerializer.Deserialize<OnboardingProfileDto>(status.ProfileJson),
            Wallet = string.IsNullOrEmpty(status.IncomeJson) 
                ? null 
                : JsonSerializer.Deserialize<OnboardingWalletDto>(status.IncomeJson),
            CategorySetup = string.IsNullOrEmpty(status.ExpensesJson) 
                ? null 
                : JsonSerializer.Deserialize<OnboardingCategorySetupDto>(status.ExpensesJson),
            SavingsGoal = string.IsNullOrEmpty(status.GoalsJson) 
                ? null 
                : JsonSerializer.Deserialize<OnboardingSavingsGoalDto>(status.GoalsJson)
        };
    }

    /// <summary>
    /// Initialize onboarding for a new user
    /// </summary>
    public async Task<OnboardingStatusDto> InitializeOnboardingAsync(long userId)
    {
        var existing = await _context.OnboardingStatuses
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (existing != null)
        {
            return await GetOnboardingStatusAsync(userId) ?? new OnboardingStatusDto();
        }

        var status = new OnboardingStatus
        {
            UserId = userId,
            CurrentStep = (int)OnboardingStep.Welcome,
            IsCompleted = false,
            StartedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OnboardingStatuses.Add(status);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Initialized onboarding for user {userId}");

        return new OnboardingStatusDto
        {
            Id = status.Id,
            UserId = status.UserId,
            CurrentStep = status.CurrentStep,
            IsCompleted = status.IsCompleted,
            StartedAt = status.StartedAt,
            UpdatedAt = status.UpdatedAt
        };
    }

    /// <summary>
    /// Update onboarding step
    /// </summary>
    public async Task<bool> UpdateStepAsync(long userId, int step, string? stepData = null)
    {
        var status = await _context.OnboardingStatuses
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (status == null)
        {
            return false;
        }

        status.CurrentStep = step;
        status.UpdatedAt = DateTime.UtcNow;

        // Store step data in appropriate JSON field
        if (!string.IsNullOrEmpty(stepData))
        {
            switch ((OnboardingStep)step)
            {
                case OnboardingStep.BasicSettings:
                    status.ProfileJson = stepData;
                    break;
                case OnboardingStep.CreateWallet:
                    status.IncomeJson = stepData;
                    break;
                case OnboardingStep.SetupCategories:
                    status.ExpensesJson = stepData;
                    break;
                case OnboardingStep.SavingsGoal:
                    status.GoalsJson = stepData;
                    break;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Updated onboarding step to {step} for user {userId}");

        return true;
    }

    /// <summary>
    /// Complete onboarding and create initial data
    /// </summary>
    public async Task<bool> CompleteOnboardingAsync(long userId, CompleteOnboardingDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Update user settings
            user.DefaultCurrency = dto.Profile.Currency;
            user.Language = dto.Profile.Language;
            user.Timezone = dto.Profile.Timezone ?? "UTC";
            user.Theme = dto.Profile.Theme ?? "light";
            user.OnboardingCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            // Create first wallet/account
            var account = new Account
            {
                UserId = userId,
                Name = dto.Wallet.Name,
                AccountType = dto.Wallet.AccountType,
                InitialBalance = dto.Wallet.InitialBalance,
                CurrentBalance = dto.Wallet.InitialBalance,
                Currency = dto.Profile.Currency,
                Icon = dto.Wallet.Icon ?? "💰",
                Color = dto.Wallet.Color ?? "#4CAF50",
                IsActive = true,
                IncludeInTotal = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(); // Save to get account ID

            // Create initial balance transaction if balance > 0
            if (dto.Wallet.InitialBalance > 0)
            {
                var initialTransaction = new Transaction
                {
                    UserId = userId,
                    AccountId = account.Id,
                    TransactionType = (int)TransactionType.Income,
                    Amount = dto.Wallet.InitialBalance,
                    Currency = dto.Profile.Currency,
                    Note = "Initial Balance",
                    TransactionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(initialTransaction);
            }

            // Create categories based on template
            var categories = GetCategoriesByTemplate(dto.CategorySetup.Template, userId);
            _context.Categories.AddRange(categories);

            // Add custom categories if any
            if (dto.CategorySetup.CustomCategories != null && dto.CategorySetup.CustomCategories.Any())
            {
                foreach (var customCat in dto.CategorySetup.CustomCategories)
                {
                    var category = new Category
                    {
                        UserId = userId,
                        Name = customCat.Name,
                        Type = customCat.Type,
                        Icon = customCat.Icon,
                        Color = customCat.Color,
                        Description = customCat.Description,
                        IsDefault = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Categories.Add(category);
                }
            }

            // Create savings goal if provided
            if (dto.SavingsGoal != null && !string.IsNullOrEmpty(dto.SavingsGoal.Name))
            {
                var savingsGoal = new SavingsGoal
                {
                    UserId = userId,
                    Name = dto.SavingsGoal.Name,
                    TargetAmount = dto.SavingsGoal.TargetAmount ?? 0,
                    CurrentAmount = 0,
                    TargetDate = dto.SavingsGoal.TargetDate,
                    Icon = dto.SavingsGoal.Icon ?? "🎯",
                    Color = dto.SavingsGoal.Color ?? "#2196F3",
                    Status = 0, // Active
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SavingsGoals.Add(savingsGoal);
            }

            // Update onboarding status
            var status = await _context.OnboardingStatuses
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (status != null)
            {
                status.CurrentStep = (int)OnboardingStep.Completed;
                status.IsCompleted = true;
                status.CompletedAt = DateTime.UtcNow;
                status.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Completed onboarding for user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Error completing onboarding for user {userId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get category templates by type
    /// </summary>
    public List<CategoryPreviewDto> GetCategoryTemplatePreview(string template)
    {
        var categories = new List<CategoryPreviewDto>();

        switch (template.ToLower())
        {
            case "student":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Tuition", Type = 0, Icon = "🎓", Color = "#FF5722", Description = "School fees and tuition" },
                    new() { Name = "Books & Supplies", Type = 0, Icon = "📚", Color = "#FF9800", Description = "Books and study materials" },
                    new() { Name = "Food & Drinks", Type = 0, Icon = "🍔", Color = "#FFC107", Description = "Meals and beverages" },
                    new() { Name = "Transportation", Type = 0, Icon = "🚌", Color = "#9C27B0", Description = "Bus, taxi, etc." },
                    new() { Name = "Entertainment", Type = 0, Icon = "🎮", Color = "#E91E63", Description = "Movies, games, etc." },
                    new() { Name = "Scholarship", Type = 1, Icon = "🏆", Color = "#4CAF50", Description = "Scholarship income" },
                    new() { Name = "Part-time Job", Type = 1, Icon = "💼", Color = "#8BC34A", Description = "Part-time work income" },
                    new() { Name = "Allowance", Type = 1, Icon = "💰", Color = "#CDDC39", Description = "Monthly allowance" }
                };
                break;

            case "family":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Groceries", Type = 0, Icon = "🛒", Color = "#4CAF50", Description = "Food and household items" },
                    new() { Name = "Utilities", Type = 0, Icon = "💡", Color = "#FF9800", Description = "Electricity, water, internet" },
                    new() { Name = "Rent/Mortgage", Type = 0, Icon = "🏠", Color = "#F44336", Description = "Housing costs" },
                    new() { Name = "Healthcare", Type = 0, Icon = "🏥", Color = "#2196F3", Description = "Medical expenses" },
                    new() { Name = "Education", Type = 0, Icon = "🎓", Color = "#9C27B0", Description = "Children's education" },
                    new() { Name = "Transportation", Type = 0, Icon = "🚗", Color = "#607D8B", Description = "Car, fuel, public transport" },
                    new() { Name = "Salary", Type = 1, Icon = "💵", Color = "#4CAF50", Description = "Monthly salary" },
                    new() { Name = "Bonus", Type = 1, Icon = "🎁", Color = "#8BC34A", Description = "Bonuses and rewards" }
                };
                break;

            case "business":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Office Supplies", Type = 0, Icon = "📎", Color = "#607D8B", Description = "Office materials" },
                    new() { Name = "Marketing", Type = 0, Icon = "📢", Color = "#FF5722", Description = "Advertising and promotion" },
                    new() { Name = "Salaries", Type = 0, Icon = "👥", Color = "#F44336", Description = "Employee salaries" },
                    new() { Name = "Rent", Type = 0, Icon = "🏢", Color = "#FF9800", Description = "Office rent" },
                    new() { Name = "Equipment", Type = 0, Icon = "💻", Color = "#9C27B0", Description = "Business equipment" },
                    new() { Name = "Sales Revenue", Type = 1, Icon = "💰", Color = "#4CAF50", Description = "Product/service sales" },
                    new() { Name = "Consulting", Type = 1, Icon = "🤝", Color = "#8BC34A", Description = "Consulting income" },
                    new() { Name = "Investment", Type = 1, Icon = "📈", Color = "#00BCD4", Description = "Investment returns" }
                };
                break;

            case "freelancer":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Software & Tools", Type = 0, Icon = "🛠️", Color = "#2196F3", Description = "Professional tools" },
                    new() { Name = "Internet & Phone", Type = 0, Icon = "📱", Color = "#FF9800", Description = "Communication costs" },
                    new() { Name = "Marketing", Type = 0, Icon = "📢", Color = "#E91E63", Description = "Self-promotion" },
                    new() { Name = "Education", Type = 0, Icon = "📖", Color = "#9C27B0", Description = "Courses and training" },
                    new() { Name = "Project Income", Type = 1, Icon = "💼", Color = "#4CAF50", Description = "Client projects" },
                    new() { Name = "Royalties", Type = 1, Icon = "🎨", Color = "#8BC34A", Description = "Royalty income" }
                };
                break;

            case "minimal":
            default:
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Food", Type = 0, Icon = "🍔", Color = "#FF5722", Description = "Food and dining" },
                    new() { Name = "Shopping", Type = 0, Icon = "🛍️", Color = "#E91E63", Description = "Shopping expenses" },
                    new() { Name = "Bills", Type = 0, Icon = "📄", Color = "#FF9800", Description = "Utility bills" },
                    new() { Name = "Other Expenses", Type = 0, Icon = "💸", Color = "#9E9E9E", Description = "Miscellaneous" },
                    new() { Name = "Salary", Type = 1, Icon = "💰", Color = "#4CAF50", Description = "Income" },
                    new() { Name = "Other Income", Type = 1, Icon = "💵", Color = "#8BC34A", Description = "Other income" }
                };
                break;
        }

        return categories;
    }

    /// <summary>
    /// Get categories by template for database insertion
    /// </summary>
    private List<Category> GetCategoriesByTemplate(string template, long userId)
    {
        var preview = GetCategoryTemplatePreview(template);
        return preview.Select(p => new Category
        {
            UserId = userId,
            Name = p.Name,
            Type = p.Type,
            Icon = p.Icon,
            Color = p.Color,
            Description = p.Description,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
    }

    /// <summary>
    /// Calculate monthly savings amount
    /// </summary>
    public decimal CalculateMonthlySavings(decimal targetAmount, DateOnly? targetDate)
    {
        if (!targetDate.HasValue || targetDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthsRemaining = ((targetDate.Value.Year - today.Year) * 12) + (targetDate.Value.Month - today.Month);

        if (monthsRemaining <= 0)
        {
            return targetAmount;
        }

        return Math.Round(targetAmount / monthsRemaining, 2);
    }
}
