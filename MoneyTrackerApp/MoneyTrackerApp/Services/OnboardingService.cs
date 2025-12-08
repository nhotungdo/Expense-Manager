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
    public async Task<(bool Success, string Message)> CompleteOnboardingAsync(long userId, CompleteOnboardingDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
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
                InitialBalance = 0, // Set to 0 because we create a transaction for this, and the trigger sums them
                CurrentBalance = 0, // Let trigger update this from the transaction
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
                    Account = account,
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
                    TargetDate = dto.SavingsGoal.TargetDate.HasValue ? DateOnly.FromDateTime(dto.SavingsGoal.TargetDate.Value) : null,
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
            return (true, "Success");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Error completing onboarding for user {userId}: {ex.Message}");
            return (false, ex.Message); // Return actual error message
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
                    new() { Name = "Học phí", Type = 0, Icon = "🎓", Color = "#FF5722", Description = "Học phí và các khoản lệ phí" },
                    new() { Name = "Sách & Dụng cụ", Type = 0, Icon = "📚", Color = "#FF9800", Description = "Sách giáo khoa và dụng cụ học tập" },
                    new() { Name = "Ăn uống", Type = 0, Icon = "🍔", Color = "#FFC107", Description = "Ăn uống hàng ngày" },
                    new() { Name = "Di chuyển", Type = 0, Icon = "🚌", Color = "#9C27B0", Description = "Xe buýt, xăng xe" },
                    new() { Name = "Giải trí", Type = 0, Icon = "🎮", Color = "#E91E63", Description = "Xem phim, chơi game, đi chơi" },
                    new() { Name = "Học bổng", Type = 1, Icon = "🏆", Color = "#4CAF50", Description = "Thu nhập từ học bổng" },
                    new() { Name = "Làm thêm", Type = 1, Icon = "💼", Color = "#8BC34A", Description = "Thu nhập từ việc làm thêm" },
                    new() { Name = "Trợ cấp", Type = 1, Icon = "💰", Color = "#CDDC39", Description = "Trợ cấp hàng tháng từ gia đình" }
                };
                break;

            case "family":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Mua sắm", Type = 0, Icon = "🛒", Color = "#4CAF50", Description = "Thực phẩm và đồ dùng gia đình" },
                    new() { Name = "Hóa đơn", Type = 0, Icon = "💡", Color = "#FF9800", Description = "Điện, nước, internet" },
                    new() { Name = "Thuê nhà", Type = 0, Icon = "🏠", Color = "#F44336", Description = "Tiền thuê nhà hoặc trả góp" },
                    new() { Name = "Sức khỏe", Type = 0, Icon = "🏥", Color = "#2196F3", Description = "Chi phí y tế" },
                    new() { Name = "Giáo dục", Type = 0, Icon = "🎓", Color = "#9C27B0", Description = "Học phí cho con" },
                    new() { Name = "Di chuyển", Type = 0, Icon = "🚗", Color = "#607D8B", Description = "Xăng xe, bảo dưỡng" },
                    new() { Name = "Lương", Type = 1, Icon = "💵", Color = "#4CAF50", Description = "Lương hàng tháng" },
                    new() { Name = "Thưởng", Type = 1, Icon = "🎁", Color = "#8BC34A", Description = "Tiền thưởng và quà tặng" }
                };
                break;

            case "business":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Văn phòng phẩm", Type = 0, Icon = "📎", Color = "#607D8B", Description = "Dụng cụ văn phòng" },
                    new() { Name = "Tiếp thị", Type = 0, Icon = "📢", Color = "#FF5722", Description = "Quảng cáo và khuyến mãi" },
                    new() { Name = "Lương nhân viên", Type = 0, Icon = "👥", Color = "#F44336", Description = "Chi trả lương nhân viên" },
                    new() { Name = "Thuê văn phòng", Type = 0, Icon = "🏢", Color = "#FF9800", Description = "Tiền thuê mặt bằng" },
                    new() { Name = "Thiết bị", Type = 0, Icon = "💻", Color = "#9C27B0", Description = "Máy móc thiết bị kinh doanh" },
                    new() { Name = "Doanh thu", Type = 1, Icon = "💰", Color = "#4CAF50", Description = "Doanh thu bán hàng" },
                    new() { Name = "Tư vấn", Type = 1, Icon = "🤝", Color = "#8BC34A", Description = "Thu nhập từ tư vấn" },
                    new() { Name = "Đầu tư", Type = 1, Icon = "📈", Color = "#00BCD4", Description = "Lợi nhuận đầu tư" }
                };
                break;

            case "freelancer":
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Phần mềm & Công cụ", Type = 0, Icon = "🛠️", Color = "#2196F3", Description = "Công cụ làm việc" },
                    new() { Name = "Internet & Điện thoại", Type = 0, Icon = "📱", Color = "#FF9800", Description = "Chi phí liên lạc" },
                    new() { Name = "Tiếp thị", Type = 0, Icon = "📢", Color = "#E91E63", Description = "Quảng bá bản thân" },
                    new() { Name = "Đào tạo", Type = 0, Icon = "📖", Color = "#9C27B0", Description = "Khóa học và nâng cao kỹ năng" },
                    new() { Name = "Dự án", Type = 1, Icon = "💼", Color = "#4CAF50", Description = "Thu nhập từ dự án" },
                    new() { Name = "Tiền bản quyền", Type = 1, Icon = "🎨", Color = "#8BC34A", Description = "Thu nhập thụ động" }
                };
                break;

            case "minimal":
            default:
                categories = new List<CategoryPreviewDto>
                {
                    new() { Name = "Ăn uống", Type = 0, Icon = "🍔", Color = "#FF5722", Description = "Ăn uống hàng ngày" },
                    new() { Name = "Mua sắm", Type = 0, Icon = "🛍️", Color = "#E91E63", Description = "Mua sắm cá nhân" },
                    new() { Name = "Hóa đơn", Type = 0, Icon = "📄", Color = "#FF9800", Description = "Điện, nước, internet" },
                    new() { Name = "Chi tiêu khác", Type = 0, Icon = "💸", Color = "#9E9E9E", Description = "Các khoản linh tinh" },
                    new() { Name = "Lương", Type = 1, Icon = "💰", Color = "#4CAF50", Description = "Thu nhập chính" },
                    new() { Name = "Thu nhập khác", Type = 1, Icon = "💵", Color = "#8BC34A", Description = "Các nguồn thu khác" }
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
