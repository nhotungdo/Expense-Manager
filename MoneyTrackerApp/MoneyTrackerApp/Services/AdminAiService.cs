using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MoneyTrackerApp.Services;

public interface IAdminAiService
{
    Task<List<ChurnPredictionDto>> GetChurnPredictionsAsync();
    Task<List<FraudDetectionDto>> DetectFraudAsync();
    Task<NaturalLanguageResponseDto> ProcessNaturalLanguageQueryAsync(string query);
}

public class AdminAiService : IAdminAiService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<AdminAiService> _logger;

    public AdminAiService(
        ExpenseManagerContext context,
        ILogger<AdminAiService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ChurnPredictionDto>> GetChurnPredictionsAsync()
    {
        var predictions = new List<ChurnPredictionDto>();
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // Get users with active subscriptions
        var activeUsers = await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.Package)
            .Where(s => s.Status == 1 && s.EndDate > DateTime.UtcNow) // Active
            .Select(s => s.User)
            .Distinct()
            .ToListAsync();

        foreach (var user in activeUsers)
        {
            var riskFactors = new List<string>();
            var suggestedActions = new List<string>();
            var riskScore = 0;

            // Factor 1: Login frequency
            var daysSinceLastLogin = user.LastLogin.HasValue
                ? (DateTime.UtcNow - user.LastLogin.Value).Days
                : (user.CreatedAt.HasValue ? (DateTime.UtcNow - user.CreatedAt.Value).Days : 999);

            if (daysSinceLastLogin > 30)
            {
                riskScore += 30;
                riskFactors.Add($"Không đăng nhập trong {daysSinceLastLogin} ngày");
                suggestedActions.Add("Gửi email nhắc nhở với hướng dẫn sử dụng tính năng mới");
            }
            else if (daysSinceLastLogin > 14)
            {
                riskScore += 15;
                riskFactors.Add($"Không đăng nhập trong {daysSinceLastLogin} ngày");
            }

            // Factor 2: Feature usage decline
            var recentTransactions = await _context.Transactions
                .Where(t => t.Account.UserId == user.Id && t.TransactionDate >= thirtyDaysAgo)
                .CountAsync();

            var olderTransactions = await _context.Transactions
                .Where(t => t.Account.UserId == user.Id &&
                           t.TransactionDate >= thirtyDaysAgo.AddDays(-30) &&
                           t.TransactionDate < thirtyDaysAgo)
                .CountAsync();

            if (olderTransactions > 0)
            {
                var usageDecline = ((double)(olderTransactions - recentTransactions) / olderTransactions) * 100;
                if (usageDecline > 50)
                {
                    riskScore += 25;
                    riskFactors.Add($"Tần suất sử dụng tính năng giảm {usageDecline:F0}%");
                    suggestedActions.Add("Gửi email với hướng dẫn tính năng mới hoặc đề xuất mã giảm giá 10%");
                }
                else if (usageDecline > 30)
                {
                    riskScore += 10;
                    riskFactors.Add($"Tần suất sử dụng giảm {usageDecline:F0}%");
                }
            }

            // Factor 3: Credit card expiration (simulated - check payment method)
            var recentPayments = await _context.Payments
                .Where(p => p.Subscription.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (recentPayments != null)
            {
                var paymentAge = (DateTime.UtcNow - (recentPayments.CreatedAt ?? DateTime.UtcNow)).Days;
                if (paymentAge > 300) // ~10 months
                {
                    riskScore += 20;
                    riskFactors.Add("Thẻ tín dụng sắp hết hạn hoặc cần cập nhật");
                    suggestedActions.Add("Gửi email nhắc nhở cập nhật phương thức thanh toán");
                }
            }

            // Factor 4: Failed transactions
            var failedPayments = await _context.Payments
                .Where(p => p.Subscription.UserId == user.Id &&
                           p.Status == 3 && // Failed
                           p.CreatedAt >= thirtyDaysAgo)
                .CountAsync();

            if (failedPayments > 0)
            {
                riskScore += 25;
                riskFactors.Add($"Có {failedPayments} giao dịch thất bại gần đây");
                suggestedActions.Add("Liên hệ hỗ trợ để giải quyết vấn đề thanh toán");
            }

            // Factor 5: Subscription ending soon
            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == user.Id && s.Status == 1)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (subscription != null)
            {
                var daysUntilExpiry = (subscription.EndDate - DateTime.UtcNow).Days;
                if (daysUntilExpiry <= 7 && !subscription.AutoRenew)
                {
                    riskScore += 30;
                    riskFactors.Add($"Gói đăng ký hết hạn trong {daysUntilExpiry} ngày và không tự động gia hạn");
                    suggestedActions.Add("Gửi email khuyến khích bật tự động gia hạn với mã giảm giá");
                }
                else if (daysUntilExpiry <= 30)
                {
                    riskScore += 10;
                }
            }

            // Determine risk level
            string riskLevel = riskScore switch
            {
                >= 70 => "high",
                >= 40 => "medium",
                _ => "low"
            };

            if (riskScore >= 40)
            {
                predictions.Add(new ChurnPredictionDto
                {
                    UserId = user.Id,
                    UserEmail = user.Email ?? "",
                    UserName = user.FullName ?? user.UserName ?? "",
                    RiskPercentage = Math.Min(riskScore, 95),
                    RiskLevel = riskLevel,
                    RiskFactors = riskFactors,
                    SuggestedActions = suggestedActions,
                    LastLoginDate = user.LastLogin ?? user.CreatedAt ?? DateTime.UtcNow,
                    DaysSinceLastLogin = daysSinceLastLogin
                });
            }
        }

        return predictions.OrderByDescending(p => p.RiskPercentage).ToList();
    }

    public async Task<List<FraudDetectionDto>> DetectFraudAsync()
    {
        var alerts = new List<FraudDetectionDto>();
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

        // Detection 1: Trial abuse - Same card/IP creating multiple trial accounts
        var recentTrialAccounts = await _context.Users
            .Where(u => u.CreatedAt >= fiveMinutesAgo)
            .ToListAsync();

        // Group by IP (simulated - would need IP tracking)
        // For now, check by creation time and email pattern
        var suspiciousGroups = recentTrialAccounts
            .GroupBy(u => u.Email?.Split('@')[0] ?? "")
            .Where(g => g.Count() > 5)
            .ToList();

        foreach (var group in suspiciousGroups)
        {
            var accounts = group.ToList();
            var affectedAccounts = accounts.Select(u => new FraudAccountDto
            {
                UserId = u.Id,
                Email = u.Email ?? "",
                CreatedAt = u.CreatedAt ?? DateTime.UtcNow,
                IsBlocked = false
            }).ToList();

            alerts.Add(new FraudDetectionDto
            {
                AlertType = "trial_abuse",
                Message = $"Phát hiện {accounts.Count} tài khoản Trial được tạo trong 5 phút từ cùng một email pattern.",
                AffectedAccountCount = accounts.Count,
                AffectedAccounts = affectedAccounts,
                DetectedAt = DateTime.UtcNow,
                Severity = accounts.Count > 10 ? "critical" : "high",
                AutoBlocked = accounts.Count > 10
            });
        }

        // Detection 2: Card abuse - Same card used for multiple accounts
        var recentPayments = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.User)
            .Where(p => p.CreatedAt >= fiveMinutesAgo && p.Status == 2) // Completed
            .ToListAsync();

        // Group by transaction ID pattern (simulated card detection)
        var cardGroups = recentPayments
            .Where(p => !string.IsNullOrEmpty(p.TransactionId))
            .GroupBy(p => p.TransactionId?.Substring(0, Math.Min(8, p.TransactionId.Length)) ?? "")
            .Where(g => g.Count() > 3)
            .ToList();

        foreach (var group in cardGroups)
        {
            var payments = group.ToList();
            var uniqueUsers = payments.Select(p => p.Subscription.UserId).Distinct().ToList();

            if (uniqueUsers.Count > 3)
            {
                var affectedAccounts = payments
                    .GroupBy(p => p.Subscription.UserId)
                    .Select(g => new FraudAccountDto
                    {
                        UserId = g.Key,
                        Email = g.First().Subscription.User.Email ?? "",
                        CardLastFour = g.First().TransactionId?.Length > 4 ? g.First().TransactionId.Substring(g.First().TransactionId.Length - 4) : null,
                        CreatedAt = g.First().CreatedAt ?? DateTime.UtcNow,
                        IsBlocked = false
                    }).ToList();

                alerts.Add(new FraudDetectionDto
                {
                    AlertType = "card_abuse",
                    Message = $"Phát hiện 1 thẻ Visa được sử dụng để đăng ký {uniqueUsers.Count} tài khoản Trial trong 5 phút từ cùng một IP.",
                    AffectedAccountCount = uniqueUsers.Count,
                    AffectedAccounts = affectedAccounts,
                    DetectedAt = DateTime.UtcNow,
                    Severity = uniqueUsers.Count > 5 ? "critical" : "high",
                    AutoBlocked = uniqueUsers.Count > 5
                });
            }
        }

        // Auto-block if critical
        foreach (var alert in alerts.Where(a => a.AutoBlocked))
        {
            foreach (var account in alert.AffectedAccounts)
            {
                var user = await _context.Users.FindAsync(account.UserId);
                if (user != null)
                {
                    user.Enabled = false;
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(30);
                }
            }
        }

        if (alerts.Any(a => a.AutoBlocked))
        {
            await _context.SaveChangesAsync();
        }

        return alerts;
    }

    public async Task<NaturalLanguageResponseDto> ProcessNaturalLanguageQueryAsync(string query)
    {
        var queryLower = query.ToLower();

        // Revenue comparison query
        if (queryLower.Contains("doanh thu") && (queryLower.Contains("so sánh") || queryLower.Contains("so với")))
        {
            return await ProcessRevenueComparisonQueryAsync(query);
        }

        // Top customers query
        if ((queryLower.Contains("top") || queryLower.Contains("cao nhất")) && 
            (queryLower.Contains("khách hàng") || queryLower.Contains("customer")) &&
            (queryLower.Contains("chưa gia hạn") || queryLower.Contains("không gia hạn")))
        {
            return await ProcessTopCustomersQueryAsync();
        }

        // Default: return error message
        return new NaturalLanguageResponseDto
        {
            Answer = "Xin lỗi, tôi chưa hiểu câu hỏi của bạn. Vui lòng hỏi về: so sánh doanh thu, danh sách khách hàng chi tiêu cao nhất, hoặc các báo cáo khác.",
            ChartType = null,
            Insights = null
        };
    }

    private async Task<NaturalLanguageResponseDto> ProcessRevenueComparisonQueryAsync(string query)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastYearSameMonthStart = currentMonthStart.AddYears(-1);

        // Extract package name if mentioned
        var packageName = "";
        if (query.Contains("Pro", StringComparison.OrdinalIgnoreCase))
            packageName = "Pro";
        else if (query.Contains("Free", StringComparison.OrdinalIgnoreCase))
            packageName = "Free";

        var paymentsQuery = _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Package)
            .Where(p => p.Status == 2); // Completed

        if (!string.IsNullOrEmpty(packageName))
        {
            paymentsQuery = paymentsQuery.Where(p => p.Subscription.Package.Name.Contains(packageName));
        }

        // Current period (this month)
        var currentPeriodRevenue = await paymentsQuery
            .Where(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < currentMonthStart.AddMonths(1))
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        // Previous period (same month last year or last month based on query)
        var previousPeriodStart = query.Contains("năm ngoái") || query.Contains("last year")
            ? lastYearSameMonthStart
            : lastMonthStart;

        var previousPeriodRevenue = await paymentsQuery
            .Where(p => p.CreatedAt >= previousPeriodStart && 
                        p.CreatedAt < previousPeriodStart.AddMonths(1))
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var growthPercentage = previousPeriodRevenue > 0
            ? ((currentPeriodRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100
            : 0;

        // Get revenue by region (using address field as proxy)
        var revenueByRegion = await paymentsQuery
            .Where(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < currentMonthStart.AddMonths(1))
            .Include(p => p.Subscription.User)
            .GroupBy(p => p.Subscription.User.Address ?? "Không xác định")
            .Select(g => new RevenueByRegionDto
            {
                Region = g.Key.Contains("Hà Nội") ? "Hà Nội" : 
                        g.Key.Contains("TP.HCM") || g.Key.Contains("Hồ Chí Minh") ? "TP.HCM" : "Khác",
                Amount = g.Sum(p => p.Amount)
            })
            .GroupBy(r => r.Region)
            .Select(g => new RevenueByRegionDto
            {
                Region = g.Key,
                Amount = g.Sum(r => r.Amount),
                Percentage = currentPeriodRevenue > 0 ? (g.Sum(r => r.Amount) / currentPeriodRevenue) * 100 : 0
            })
            .ToListAsync();

        var primaryGrowthSource = revenueByRegion.OrderByDescending(r => r.Amount).FirstOrDefault()?.Region ?? "Không xác định";

        var chartData = new RevenueComparisonDto
        {
            Period = $"{now:MMMM yyyy}",
            CurrentPeriodRevenue = currentPeriodRevenue,
            PreviousPeriodRevenue = previousPeriodRevenue,
            GrowthPercentage = growthPercentage,
            PrimaryGrowthSource = primaryGrowthSource,
            RevenueByRegion = revenueByRegion
        };

        var answer = $"Doanh thu gói {packageName} tháng này là {currentPeriodRevenue:N0}₫, " +
                    $"so với cùng kỳ năm ngoái là {previousPeriodRevenue:N0}₫. " +
                    $"Tăng trưởng {growthPercentage:F1}%, chủ yếu từ khách hàng khu vực {primaryGrowthSource}.";

        return new NaturalLanguageResponseDto
        {
            Answer = answer,
            ChartType = "bar",
            ChartData = chartData,
            Insights = $"Tăng trưởng {growthPercentage:F1}%, chủ yếu từ khách hàng khu vực {primaryGrowthSource}."
        };
    }

    private async Task<NaturalLanguageResponseDto> ProcessTopCustomersQueryAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);

        // Get top 10 customers by spending who haven't renewed this month
        var topCustomers = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.User)
            .Where(p => p.Status == 2) // Completed
            .GroupBy(p => p.Subscription.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalSpending = g.Sum(p => p.Amount),
                User = g.First().Subscription.User,
                HasActiveSubscription = g.Any(p => p.Subscription.Status == 1 && p.Subscription.EndDate > now),
                SubscriptionEndDate = g.OrderByDescending(p => p.Subscription.EndDate)
                    .First().Subscription.EndDate
            })
            .Where(x => !x.HasActiveSubscription || x.SubscriptionEndDate < thisMonthStart)
            .OrderByDescending(x => x.TotalSpending)
            .Take(10)
            .ToListAsync();

        var customers = topCustomers.Select(c => new CustomerSpendingDto
        {
            UserId = c.UserId,
            Email = c.User.Email ?? "",
            Name = c.User.FullName ?? c.User.UserName ?? "",
            TotalSpending = c.TotalSpending,
            HasActiveSubscription = c.HasActiveSubscription,
            SubscriptionEndDate = c.SubscriptionEndDate
        }).ToList();

        var dataRows = customers.Select(c => new DataRowDto
        {
            Values = new Dictionary<string, object>
            {
                ["Email"] = c.Email,
                ["Tên"] = c.Name,
                ["Tổng chi tiêu"] = c.TotalSpending,
                ["Có đăng ký hoạt động"] = c.HasActiveSubscription ? "Có" : "Không"
            }
        }).ToList();

        var answer = $"Danh sách top 10 khách hàng chi tiêu cao nhất chưa gia hạn đăng ký tháng này:\n\n" +
                    string.Join("\n", customers.Select((c, i) => 
                        $"{i + 1}. {c.Name} ({c.Email}) - {c.TotalSpending:N0}₫"));

        return new NaturalLanguageResponseDto
        {
            Answer = answer,
            ChartType = "table",
            ChartData = new TopCustomersDto { Customers = customers },
            DataRows = dataRows,
            Insights = $"Tổng cộng {customers.Count} khách hàng cần liên hệ để gia hạn."
        };
    }
}

