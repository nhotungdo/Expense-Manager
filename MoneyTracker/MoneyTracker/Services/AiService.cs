using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class AiService : IAiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;
    private readonly ILogger<AiService> _logger;

    public AiService(IUnitOfWork unitOfWork, ITransactionService transactionService, IBudgetService budgetService, ILogger<AiService> logger)
    {
        _unitOfWork = unitOfWork;
        _transactionService = transactionService;
        _budgetService = budgetService;
        _logger = logger;
    }

    public async Task<IEnumerable<AiSuggestion>> GetSuggestionsAsync(long userId)
    {
        var suggestions = new List<AiSuggestion>();

        // Get budget suggestions
        var budgetSuggestions = await GetBudgetSuggestionsAsync(userId);
        suggestions.AddRange(budgetSuggestions);

        // Get spending suggestions
        var spendingSuggestions = await GetSpendingSuggestionsAsync(userId);
        suggestions.AddRange(spendingSuggestions);

        // Get savings suggestions
        var savingsSuggestions = await GetSavingsSuggestionsAsync(userId);
        suggestions.AddRange(savingsSuggestions);

        return suggestions.OrderByDescending(s => s.CreatedAt).Take(10);
    }

    public async Task<AiSuggestion> GenerateSuggestionAsync(long userId, string suggestionType)
    {
        var suggestion = suggestionType.ToLower() switch
        {
            "budget" => await GenerateBudgetSuggestionAsync(userId),
            "spending" => await GenerateSpendingSuggestionAsync(userId),
            "savings" => await GenerateSavingsSuggestionAsync(userId),
            _ => await GenerateGeneralSuggestionAsync(userId)
        };

        await _unitOfWork.AiSuggestions.AddAsync(suggestion);
        await _unitOfWork.SaveChangesAsync();

        return suggestion;
    }

    public async Task<IEnumerable<AiSuggestion>> GetBudgetSuggestionsAsync(long userId)
    {
        var activeBudgets = await _budgetService.GetActiveBudgetsAsync(userId);
        var suggestions = new List<AiSuggestion>();

        foreach (var budget in activeBudgets)
        {
            // Calculate spent amount dynamically
            var spentAmount = await CalculateSpentAmountAsync(userId, budget.CategoryId, budget.StartDate, budget.EndDate);
            var utilizationRate = budget.Amount > 0 ? (spentAmount / budget.Amount) * 100 : 0;

            if (utilizationRate >= 90)
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"⚠️ Bạn đã chi tiêu {utilizationRate:F1}% ngân sách '{budget.Category?.Name ?? "Tổng"}' trong tháng này. Hãy cẩn thận với các khoản chi tiêu tiếp theo!",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (utilizationRate >= 75)
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"💡 Bạn đã chi tiêu {utilizationRate:F1}% ngân sách '{budget.Category?.Name ?? "Tổng"}'. Còn lại {budget.Amount - spentAmount:C0} để chi tiêu trong tháng này.",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return suggestions;
    }

    public async Task<IEnumerable<AiSuggestion>> GetSpendingSuggestionsAsync(long userId)
    {
        var suggestions = new List<AiSuggestion>();
        var (startDate, endDate) = GetCurrentMonthDates();
        var categoryBreakdown = await _transactionService.GetCategoryBreakdownAsync(userId, startDate, endDate);

        var topCategory = categoryBreakdown.FirstOrDefault();
        if (topCategory != null)
        {
            suggestions.Add(new AiSuggestion
            {
                UserId = userId,
                Suggestion = $"📊 Danh mục chi tiêu nhiều nhất tháng này: {topCategory.CategoryName} ({topCategory.TotalAmount:C0})",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check for unusual spending patterns
        var totalExpense = categoryBreakdown.Sum(c => c.TotalAmount);
        if (totalExpense > 0)
        {
            var averageDailyExpense = totalExpense / DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            if (averageDailyExpense > 500000) // 500k VND per day
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = "💰 Chi tiêu hàng ngày của bạn khá cao. Hãy xem xét tối ưu hóa các khoản chi tiêu không cần thiết.",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return suggestions;
    }

    public async Task<IEnumerable<AiSuggestion>> GetSavingsSuggestionsAsync(long userId)
    {
        var suggestions = new List<AiSuggestion>();
        var (startDate, endDate) = GetCurrentMonthDates();
        var (totalIncome, totalExpense, netIncome) = await _transactionService.GetUserSummaryAsync(userId, startDate, endDate);

        if (netIncome > 0)
        {
            var savingsRate = (netIncome / totalIncome) * 100;
            if (savingsRate >= 20)
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"🎉 Tuyệt vời! Bạn đã tiết kiệm được {savingsRate:F1}% thu nhập tháng này ({netIncome:C0}). Hãy tiếp tục duy trì thói quen tốt này!",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (savingsRate >= 10)
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"👍 Bạn đã tiết kiệm được {savingsRate:F1}% thu nhập tháng này ({netIncome:C0}). Hãy cố gắng tăng tỷ lệ tiết kiệm lên 20%!",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (savingsRate > 0)
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"💪 Bạn đã tiết kiệm được {netIncome:C0} tháng này. Hãy cố gắng tăng tỷ lệ tiết kiệm lên ít nhất 10% thu nhập!",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                suggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = "⚠️ Chi tiêu của bạn vượt quá thu nhập tháng này. Hãy xem xét cắt giảm các khoản chi tiêu không cần thiết và tạo ngân sách chi tiêu hợp lý.",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return suggestions;
    }

    private async Task<AiSuggestion> GenerateBudgetSuggestionAsync(long userId)
    {
        var activeBudgets = await _budgetService.GetActiveBudgetsAsync(userId);
        var totalBudget = activeBudgets.Sum(b => b.Amount);

        // Calculate total spent amount dynamically
        var totalSpent = 0m;
        foreach (var budget in activeBudgets)
        {
            var spentAmount = await CalculateSpentAmountAsync(userId, budget.CategoryId, budget.StartDate, budget.EndDate);
            totalSpent += spentAmount;
        }

        var suggestion = new AiSuggestion
        {
            UserId = userId,
            Suggestion = $"📋 Tổng ngân sách: {totalBudget:C0}, Đã chi: {totalSpent:C0}, Còn lại: {totalBudget - totalSpent:C0}",
            CreatedAt = DateTime.UtcNow
        };

        return suggestion;
    }

    private async Task<AiSuggestion> GenerateSpendingSuggestionAsync(long userId)
    {
        var (startDate, endDate) = GetCurrentMonthDates();
        var categoryBreakdown = await _transactionService.GetCategoryBreakdownAsync(userId, startDate, endDate);

        var suggestion = new AiSuggestion
        {
            UserId = userId,
            Suggestion = $"📈 Phân tích chi tiêu: {categoryBreakdown.Count()} danh mục, Tổng chi: {categoryBreakdown.Sum(c => c.TotalAmount):C0}",
            CreatedAt = DateTime.UtcNow
        };

        return suggestion;
    }

    private async Task<AiSuggestion> GenerateSavingsSuggestionAsync(long userId)
    {
        var (startDate, endDate) = GetCurrentMonthDates();
        var (totalIncome, totalExpense, netIncome) = await _transactionService.GetUserSummaryAsync(userId, startDate, endDate);

        var suggestion = new AiSuggestion
        {
            UserId = userId,
            Suggestion = $"💰 Tình hình tài chính: Thu {totalIncome:C0}, Chi {totalExpense:C0}, Tiết kiệm {netIncome:C0}",
            CreatedAt = DateTime.UtcNow
        };

        return suggestion;
    }

    private Task<AiSuggestion> GenerateGeneralSuggestionAsync(long userId)
    {
        var suggestion = new AiSuggestion
        {
            UserId = userId,
            Suggestion = "💡 Hãy thường xuyên kiểm tra báo cáo chi tiêu để quản lý tài chính hiệu quả hơn!",
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(suggestion);
    }

    private (DateTime startDate, DateTime endDate) GetCurrentMonthDates()
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return (startDate, endDate);
    }

    private async Task<decimal> CalculateSpentAmountAsync(long userId, long? categoryId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t =>
            t.UserId == userId &&
            t.Type == TransactionType.Expense &&
            t.TransactionDate >= startDate &&
            t.TransactionDate <= endDate &&
            (categoryId == null || t.CategoryId == categoryId));

        return transactions.Sum(t => t.Amount);
    }
}
