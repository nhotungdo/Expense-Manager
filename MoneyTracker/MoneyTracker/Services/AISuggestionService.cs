using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class AISuggestionService : IAISuggestionService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<AISuggestionService> _logger;
        private readonly IAuditService _auditService;

        public AISuggestionService(ExpenseManagerContext context, ILogger<AISuggestionService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IEnumerable<AiSuggestion>> GetSuggestionsAsync(long userId, int skip = 0, int take = 10)
        {
            return await _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<AiSuggestion> GenerateSuggestionAsync(long userId)
        {
            var analysis = await GetSpendingAnalysisAsync(userId);
            var recommendations = await GenerateBudgetRecommendationsAsync(userId);
            var insights = await GenerateSpendingInsightsAsync(userId);

            var allSuggestions = new List<string>();
            allSuggestions.AddRange(recommendations);
            allSuggestions.AddRange(insights);

            var suggestion = new AiSuggestion
            {
                UserId = userId,
                Suggestion = string.Join(" ", allSuggestions),
                CreatedAt = DateTime.UtcNow
            };

            _context.AiSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "GENERATE_AI_SUGGESTION", "AI suggestion generated", "AiSuggestion", suggestion.Id);

            _logger.LogInformation("AI suggestion generated for user {UserId}: {SuggestionId}", userId, suggestion.Id);
            return suggestion;
        }

        public async Task<bool> MarkSuggestionAsReadAsync(long suggestionId, long userId)
        {
            var suggestion = await _context.AiSuggestions
                .FirstOrDefaultAsync(s => s.Id == suggestionId && s.UserId == userId);

            if (suggestion == null)
                return false;

            // Note: AiSuggestion model doesn't have IsRead property, but we can add it if needed
            // For now, we'll just log the action
            await _auditService.LogUserActionAsync(userId, "READ_AI_SUGGESTION", $"Read AI suggestion: {suggestionId}", "AiSuggestion", suggestionId);

            _logger.LogInformation("AI suggestion marked as read: {SuggestionId} by user {UserId}", suggestionId, userId);
            return true;
        }

        public async Task<Dictionary<string, object>> GetSpendingAnalysisAsync(long userId)
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Get current month data
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate.Month == currentMonth &&
                           e.ExpenseDate.Year == currentYear)
                .Include(e => e.Category)
                .ToListAsync();

            var monthlyIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                           i.IncomeDate.Month == currentMonth &&
                           i.IncomeDate.Year == currentYear)
                .SumAsync(i => i.Amount);

            // Get last 3 months average
            var last3MonthsExpenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.ExpenseDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)) &&
                           e.ExpenseDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                .Include(e => e.Category)
                .ToListAsync();

            var totalMonthlyExpenses = monthlyExpenses.Sum(e => e.Amount);
            var avgMonthlyExpenses = last3MonthsExpenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => g.Sum(e => e.Amount))
                .DefaultIfEmpty(0)
                .Average();

            var expensesByCategory = monthlyExpenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            var expenseRatio = monthlyIncome > 0 ? (totalMonthlyExpenses / monthlyIncome) * 100 : 0;
            var savingsRate = monthlyIncome > 0 ? ((monthlyIncome - totalMonthlyExpenses) / monthlyIncome) * 100 : 0;

            return new Dictionary<string, object>
            {
                ["MonthlyIncome"] = monthlyIncome,
                ["MonthlyExpenses"] = totalMonthlyExpenses,
                ["AverageMonthlyExpenses"] = avgMonthlyExpenses,
                ["ExpenseRatio"] = Math.Round(expenseRatio, 2),
                ["SavingsRate"] = Math.Round(savingsRate, 2),
                ["ExpensesByCategory"] = expensesByCategory,
                ["TransactionCount"] = monthlyExpenses.Count
            };
        }

        public async Task<List<string>> GenerateBudgetRecommendationsAsync(long userId)
        {
            var analysis = await GetSpendingAnalysisAsync(userId);
            var recommendations = new List<string>();

            var expenseRatio = (decimal)analysis["ExpenseRatio"];
            var savingsRate = (decimal)analysis["SavingsRate"];
            var monthlyIncome = (decimal)analysis["MonthlyIncome"];
            var monthlyExpenses = (decimal)analysis["MonthlyExpenses"];

            if (expenseRatio > 90)
            {
                recommendations.Add("🚨 Cảnh báo: Chi tiêu tháng này đã vượt quá 90% thu nhập. Cần cắt giảm chi tiêu ngay lập tức.");
                recommendations.Add("💡 Gợi ý: Hãy xem xét các khoản chi tiêu không cần thiết và tạm dừng các mua sắm lớn.");
            }
            else if (expenseRatio > 80)
            {
                recommendations.Add("⚠️ Cảnh báo: Chi tiêu đã vượt quá 80% thu nhập. Cần kiểm soát chi tiêu tốt hơn.");
                recommendations.Add("💡 Gợi ý: Hãy lập danh sách ưu tiên cho các khoản chi tiêu và cắt giảm những thứ không quan trọng.");
            }
            else if (expenseRatio > 70)
            {
                recommendations.Add("📊 Chú ý: Chi tiêu đã vượt quá 70% thu nhập. Cần theo dõi chi tiêu cẩn thận hơn.");
                recommendations.Add("💡 Gợi ý: Hãy đặt mục tiêu tiết kiệm ít nhất 20% thu nhập mỗi tháng.");
            }
            else
            {
                recommendations.Add("✅ Tuyệt vời! Bạn đang quản lý chi tiêu tốt.");
                if (savingsRate > 20)
                {
                    recommendations.Add("💰 Tỷ lệ tiết kiệm của bạn rất tốt. Hãy xem xét đầu tư số tiền tiết kiệm này.");
                }
                else if (savingsRate > 10)
                {
                    recommendations.Add("📈 Tỷ lệ tiết kiệm khá tốt. Hãy cố gắng tăng lên 20% để có tương lai tài chính vững chắc.");
                }
                else
                {
                    recommendations.Add("🎯 Hãy cố gắng tăng tỷ lệ tiết kiệm lên ít nhất 10-20% thu nhập.");
                }
            }

            return recommendations;
        }

        public async Task<List<string>> GenerateSpendingInsightsAsync(long userId)
        {
            var analysis = await GetSpendingAnalysisAsync(userId);
            var insights = new List<string>();

            var expensesByCategory = (Dictionary<string, decimal>)analysis["ExpensesByCategory"];
            var totalExpenses = (decimal)analysis["MonthlyExpenses"];

            if (expensesByCategory.Any())
            {
                var topCategory = expensesByCategory.OrderByDescending(x => x.Value).First();
                var topCategoryPercentage = (topCategory.Value / totalExpenses) * 100;

                if (topCategoryPercentage > 40)
                {
                    insights.Add($"📊 Chi tiêu cho '{topCategory.Key}' chiếm {topCategoryPercentage:F1}% tổng chi tiêu. Hãy xem xét có thể tiết kiệm ở danh mục này.");
                }

                // Check for unusual spending patterns
                var categories = expensesByCategory.OrderByDescending(x => x.Value).Take(3);
                foreach (var category in categories)
                {
                    var percentage = (category.Value / totalExpenses) * 100;
                    if (percentage > 30)
                    {
                        insights.Add($"💡 '{category.Key}' là danh mục chi tiêu lớn nhất ({percentage:F1}%). Cân nhắc tối ưu hóa chi tiêu này.");
                    }
                }
            }

            // Check spending trends
            var avgExpenses = (decimal)analysis["AverageMonthlyExpenses"];
            var currentExpenses = (decimal)analysis["MonthlyExpenses"];

            if (currentExpenses > avgExpenses * 1.2m)
            {
                insights.Add($"📈 Chi tiêu tháng này cao hơn 20% so với trung bình 3 tháng gần đây. Hãy kiểm tra các khoản chi bất thường.");
            }
            else if (currentExpenses < avgExpenses * 0.8m)
            {
                insights.Add($"📉 Chi tiêu tháng này thấp hơn 20% so với trung bình. Tuyệt vời! Hãy duy trì thói quen tiết kiệm này.");
            }

            return insights;
        }
    }
}
