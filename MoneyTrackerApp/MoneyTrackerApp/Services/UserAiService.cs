using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System.Text.Json;

namespace MoneyTrackerApp.Services;

public interface IUserAiService
{
    Task<PlanRecommendationDto> GetPlanRecommendationAsync(long userId);
    Task<BillExplanationDto> ExplainBillAsync(long userId, decimal currentAmount);
    Task<SpendingForecastDto> GetSpendingForecastAsync(long userId);
    Task<TransactionSearchResultDto> SearchTransactionsAsync(long userId, string query);
    Task<string> AnswerTransactionQuestionAsync(long userId, string question);
}

public class UserAiService : IUserAiService
{
    private readonly ExpenseManagerContext _context;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<UserAiService> _logger;

    public UserAiService(
        ExpenseManagerContext context,
        ISubscriptionService subscriptionService,
        ILogger<UserAiService> logger)
    {
        _context = context;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<PlanRecommendationDto> GetPlanRecommendationAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new ArgumentException("User not found");

        var activeSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
        var allPackages = await _subscriptionService.GetAllPackagesAsync();
        
        // Analyze user behavior
        var limitHits = await AnalyzeLimitHitsAsync(userId);
        var currentPackage = activeSubscription != null 
            ? allPackages.FirstOrDefault(p => p.Id == activeSubscription.PackageId)
            : allPackages.FirstOrDefault(p => p.Price == 0); // Free plan

        // Behavior analysis: Check if user hits limits frequently
        if (limitHits.Count >= 5 && currentPackage?.Price == 0)
        {
            var proPackage = allPackages.FirstOrDefault(p => p.Name.Contains("Pro", StringComparison.OrdinalIgnoreCase));
            if (proPackage != null)
            {
                return new PlanRecommendationDto
                {
                    Message = $"Bạn đã đạt giới hạn {limitHits.Count} lần trong tuần này. Nâng cấp lên gói Pro sẽ giúp bạn tiết kiệm thời gian hơn.",
                    RecommendationType = "upgrade",
                    RecommendedPackageId = proPackage.Id,
                    ActionUrl = $"/Subscription/Index?packageId={proPackage.Id}"
                };
            }
        }

        // Savings calculator: Compare monthly vs annual
        if (activeSubscription != null && currentPackage != null)
        {
            var monthlyPrice = currentPackage.Price;
            var annualPackage = allPackages.FirstOrDefault(p => 
                p.DurationDays == 365 && 
                p.Name.Contains(currentPackage.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase));

            if (annualPackage != null && monthlyPrice > 0)
            {
                var annualMonthlyEquivalent = annualPackage.Price / 12;
                var savings = (monthlyPrice - annualMonthlyEquivalent) * 12;

                if (savings > 0)
                {
                    return new PlanRecommendationDto
                    {
                        Message = $"Bạn đang trả {monthlyPrice:N0}₫/tháng. Nếu chuyển sang gói năm ngay bây giờ, bạn sẽ tiết kiệm {savings:N0}₫ (tương đương {Math.Round(savings / monthlyPrice)} tháng miễn phí). Bạn có muốn chuyển đổi không?",
                        RecommendationType = "savings",
                        RecommendedPackageId = annualPackage.Id,
                        PotentialSavings = savings,
                        ActionUrl = $"/Subscription/Index?packageId={annualPackage.Id}"
                    };
                }
            }
        }

        return new PlanRecommendationDto
        {
            Message = "Gói dịch vụ hiện tại của bạn phù hợp với nhu cầu sử dụng.",
            RecommendationType = "info"
        };
    }

    public async Task<BillExplanationDto> ExplainBillAsync(long userId, decimal currentAmount)
    {
        var currentMonth = DateTime.UtcNow;
        var previousMonth = currentMonth.AddMonths(-1);

        // Get current month payments
        var currentMonthStartDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        var currentMonthEndDate = currentMonthStartDate.AddMonths(1);
        
        var currentPayments = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
            .Where(p => p.Subscription.UserId == userId &&
                       p.CreatedAt.HasValue &&
                       p.CreatedAt.Value >= currentMonthStartDate &&
                       p.CreatedAt.Value < currentMonthEndDate)
            .ToListAsync();

        // Get previous month payments
        var previousMonthStartDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
        var previousMonthEndDate = previousMonthStartDate.AddMonths(1);
        
        var previousPayments = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
            .Where(p => p.Subscription.UserId == userId &&
                       p.CreatedAt.HasValue &&
                       p.CreatedAt.Value >= previousMonthStartDate &&
                       p.CreatedAt.Value < previousMonthEndDate)
            .ToListAsync();

        var previousAmount = previousPayments.Sum(p => p.Amount);
        var changes = new List<BillChangeItemDto>();

        // Find new subscriptions or changes
        var currentSubscriptions = currentPayments.Select(p => p.Subscription).Distinct().ToList();
        var previousSubscriptions = previousPayments.Select(p => p.Subscription).Distinct().ToList();

        foreach (var sub in currentSubscriptions)
        {
            var wasInPrevious = previousSubscriptions.Any(ps => ps.Id == sub.Id);
            if (!wasInPrevious)
            {
                changes.Add(new BillChangeItemDto
                {
                    Date = sub.StartDate,
                    Description = $"Đăng ký gói mới: {sub.Package.Name}",
                    Amount = sub.Package.Price
                });
            }
        }

        // Check for additional seats or add-ons (if implemented)
        var difference = currentAmount - previousAmount;
        if (Math.Abs(difference) > 0.01m && changes.Count == 0)
        {
            changes.Add(new BillChangeItemDto
            {
                Date = currentMonth,
                Description = "Thay đổi giá gói dịch vụ hoặc phí bổ sung",
                Amount = difference
            });
        }

        var explanation = changes.Count > 0
            ? $"Bạn bị tính {currentAmount:N0}₫ tháng này thay vì {previousAmount:N0}₫ vì: " +
              string.Join(", ", changes.Select(c => $"{c.Description} (+{c.Amount:N0}₫)"))
            : $"Số tiền {currentAmount:N0}₫ là phí định kỳ cho gói dịch vụ của bạn. Gói cơ bản vẫn là {previousAmount:N0}₫.";

        return new BillExplanationDto
        {
            CurrentAmount = currentAmount,
            PreviousAmount = previousAmount,
            Explanation = explanation,
            Changes = changes
        };
    }

    public async Task<SpendingForecastDto> GetSpendingForecastAsync(long userId)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = currentMonthStart.AddMonths(1);

        // Get current month add-on spending (if applicable)
        var currentMonthSpending = await _context.Payments
            .Where(p => p.Subscription.UserId == userId &&
                       p.CreatedAt.HasValue &&
                       p.CreatedAt.Value >= currentMonthStart &&
                       p.CreatedAt.Value < nextMonthStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        // Get historical spending for trend
        var threeMonthsAgo = currentMonthStart.AddMonths(-3);
        var last3Months = await _context.Payments
            .Where(p => p.Subscription.UserId == userId &&
                       p.CreatedAt.HasValue &&
                       p.CreatedAt.Value >= threeMonthsAgo &&
                       p.CreatedAt.Value < currentMonthStart)
            .GroupBy(p => new { Year = p.CreatedAt.Value.Year, Month = p.CreatedAt.Value.Month })
            .Select(g => new { Month = g.Key, Amount = g.Sum(p => p.Amount) })
            .OrderByDescending(x => x.Month.Year)
            .ThenByDescending(x => x.Month.Month)
            .ToListAsync();

        decimal forecastedNextMonth = currentMonthSpending;
        if (last3Months.Count >= 2)
        {
            // Simple trend calculation
            var avgGrowth = last3Months.Take(2)
                .Select((x, i) => i > 0 ? (x.Amount - last3Months[i + 1].Amount) / last3Months[i + 1].Amount : 0m)
                .Average();
            forecastedNextMonth = currentMonthSpending * (1 + avgGrowth);
        }

        var message = forecastedNextMonth > currentMonthSpending * 1.1m
            ? $"Dựa trên lịch sử sử dụng Add-on của bạn, hóa đơn tháng tới dự kiến khoảng {forecastedNextMonth:N0}₫. Bạn có muốn đặt giới hạn chi tiêu/cảnh báo không?"
            : $"Dựa trên lịch sử sử dụng, hóa đơn tháng tới dự kiến khoảng {forecastedNextMonth:N0}₫.";

        return new SpendingForecastDto
        {
            CurrentMonthlySpending = currentMonthSpending,
            ForecastedNextMonth = forecastedNextMonth,
            Message = message,
            CanSetLimit = true
        };
    }

    public async Task<TransactionSearchResultDto> SearchTransactionsAsync(long userId, string query)
    {
        var queryLower = query.ToLower();
        var transactions = new List<TransactionSearchItemDto>();

        // Parse date queries like "May bill from last year"
        DateTime? targetDate = null;
        if (queryLower.Contains("tháng") || queryLower.Contains("month"))
        {
            var months = new[] { "january", "february", "march", "april", "may", "june", 
                                "july", "august", "september", "october", "november", "december" };
            var vietnameseMonths = new[] { "tháng 1", "tháng 2", "tháng 3", "tháng 4", "tháng 5", "tháng 6",
                                          "tháng 7", "tháng 8", "tháng 9", "tháng 10", "tháng 11", "tháng 12" };

            for (int i = 0; i < months.Length; i++)
            {
                if (queryLower.Contains(months[i]) || queryLower.Contains(vietnameseMonths[i]))
                {
                    var year = queryLower.Contains("năm ngoái") || queryLower.Contains("last year")
                        ? DateTime.UtcNow.Year - 1
                        : DateTime.UtcNow.Year;
                    targetDate = new DateTime(year, i + 1, 1);
                    break;
                }
            }
        }

        // Search in payments/subscriptions
        var paymentsQuery = _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
            .Where(p => p.Subscription.UserId == userId && p.CreatedAt.HasValue);

        if (targetDate.HasValue)
        {
            var targetEndDate = targetDate.Value.AddMonths(1);
            paymentsQuery = paymentsQuery.Where(p => 
                p.CreatedAt.Value >= targetDate.Value && 
                p.CreatedAt.Value < targetEndDate);
        }

        var payments = await paymentsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync();

        transactions = payments.Select(p => new TransactionSearchItemDto
        {
            Id = p.Id,
            Amount = p.Amount,
            Date = p.CreatedAt ?? DateTime.UtcNow,
            Description = $"Thanh toán gói {p.Subscription.Package.Name}",
            Category = "Subscription"
        }).ToList();

        return new TransactionSearchResultDto
        {
            Transactions = transactions,
            DownloadUrl = transactions.Count > 0 ? $"/api/Reports/export?userId={userId}&startDate={targetDate?.ToString("yyyy-MM-dd")}&endDate={targetDate?.AddMonths(1).ToString("yyyy-MM-dd")}" : null
        };
    }

    public async Task<string> AnswerTransactionQuestionAsync(long userId, string question)
    {
        var questionLower = question.ToLower();

        // Bill explanation
        if (questionLower.Contains("tại sao") && (questionLower.Contains("tính") || questionLower.Contains("phí")))
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var currentPayments = await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Subscription.UserId == userId &&
                           p.CreatedAt.HasValue &&
                           p.CreatedAt.Value >= oneMonthAgo)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var explanation = await ExplainBillAsync(userId, currentPayments);
            return explanation.Explanation;
        }

        // Spending forecast
        if (questionLower.Contains("tháng sau") || questionLower.Contains("next month"))
        {
            var forecast = await GetSpendingForecastAsync(userId);
            return forecast.Message;
        }

        // Default response
        return "Tôi có thể giúp bạn giải thích hóa đơn, dự báo chi tiêu, và tìm kiếm giao dịch. Hãy hỏi tôi cụ thể hơn!";
    }

    private async Task<List<DateTime>> AnalyzeLimitHitsAsync(long userId)
    {
        // Check transaction limits (if package has maxTransactions)
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
        if (subscription == null) return new List<DateTime>();

        var package = await _subscriptionService.GetPackageByIdAsync(subscription.PackageId);
        if (package == null || package.DurationDays == 0) return new List<DateTime>();

        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var limitHits = new List<DateTime>();

        // Check if user hit transaction limits
        var weeklyTransactions = await _context.Transactions
            .Where(t => t.Account.UserId == userId && t.TransactionDate >= weekAgo)
            .CountAsync();

        // Assuming free plan has limit of 100 transactions/month
        // This is a simplified check - adjust based on actual package limits
        if (package.Price == 0 && weeklyTransactions >= 25) // ~100/month
        {
            limitHits.Add(DateTime.UtcNow);
        }

        return limitHits;
    }
}

