using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System.Text;
using System.Text.Json;

namespace MoneyTrackerApp.Services;

public interface IAiAdvisorService
{
    Task<List<AiSuggestionDto>> GetSuggestionsAsync(long userId);
    Task GenerateSuggestionsAsync(long userId, GenerateAiSuggestionsDto? dto = null);
    Task<AiChatResponseDto> ChatAsync(long userId, string message);
    Task<AiInsightDto> GetDailyInsightAsync(long userId);
    Task<AiCashflowForecastDto> GetCashflowForecastAsync(long userId);
    Task MarkSuggestionAsReadAsync(long userId, long suggestionId);
}

public class AiAdvisorService : IAiAdvisorService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<AiAdvisorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AiAdvisorService(
        ExpenseManagerContext context, 
        ILogger<AiAdvisorService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<List<AiSuggestionDto>> GetSuggestionsAsync(long userId)
    {
        try
        {
            var suggestions = await _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .Select(s => new AiSuggestionDto
                {
                    Id = s.Id,
                    Suggestion = s.Suggestion,
                    SuggestionType = s.SuggestionType,
                    IsRead = s.IsRead,
                    CreatedAt = s.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            // If no suggestions exist, generate some default ones
            if (suggestions.Count == 0)
            {
                await GenerateDefaultSuggestionsAsync(userId);
                return await GetSuggestionsAsync(userId);
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI suggestions for user {UserId}", userId);
            throw;
        }
    }

    public async Task GenerateSuggestionsAsync(long userId, GenerateAiSuggestionsDto? dto = null)
    {
        try
        {
            // Get user's financial data
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-3);

            var transactions = await _context.Transactions
                .Where(t => t.Account.UserId == userId && t.TransactionDate >= startDate)
                .Include(t => t.Category)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);

            // Clear old suggestions
            var oldSuggestions = await _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .ToListAsync();
            _context.AiSuggestions.RemoveRange(oldSuggestions);

            var newSuggestions = new List<AiSuggestion>();

            // Generate suggestions based on spending patterns
            if (totalExpense > totalIncome)
            {
                newSuggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"Your expenses (${totalExpense:N0}) exceed your income (${totalIncome:N0}). Consider reducing discretionary spending.",
                    SuggestionType = "warning",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpense) / totalIncome * 100) : 0;
                newSuggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"Great job! You're saving {savingsRate:F1}% of your income. Keep up the good work!",
                    SuggestionType = "success",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Category-based suggestions
            var categorySpending = transactions
                .Where(t => t.TransactionType == 2)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            if (categorySpending.Any())
            {
                var topCategory = categorySpending.First();
                newSuggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = $"Your highest spending category is {topCategory.Category} (${topCategory.Total:N0}). Look for ways to optimize this expense.",
                    SuggestionType = "info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Budget suggestions
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            if (budgets.Count == 0)
            {
                newSuggestions.Add(new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = "You don't have any active budgets. Setting budgets can help you control spending and reach your financial goals.",
                    SuggestionType = "info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.AiSuggestions.AddRangeAsync(newSuggestions);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions for user {UserId}", userId);
            throw;
        }
    }

    private async Task GenerateDefaultSuggestionsAsync(long userId)
    {
        var defaultSuggestions = new List<AiSuggestion>
        {
            new AiSuggestion
            {
                UserId = userId,
                Suggestion = "Welcome! Start tracking your expenses to get personalized financial insights.",
                SuggestionType = "info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new AiSuggestion
            {
                UserId = userId,
                Suggestion = "Set up budgets for your main spending categories to better control your finances.",
                SuggestionType = "info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new AiSuggestion
            {
                UserId = userId,
                Suggestion = "Track both income and expenses regularly for accurate financial analysis.",
                SuggestionType = "success",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.AiSuggestions.AddRangeAsync(defaultSuggestions);
        await _context.SaveChangesAsync();
    }

    public async Task<AiChatResponseDto> ChatAsync(long userId, string message)
    {
        try
        {
            // Get user's financial context
            var financialData = await GetUserFinancialDataAsync(userId);
            
            // Build prompt for Gemini
            var systemPrompt = BuildSystemPrompt(financialData);
            var fullPrompt = $"{systemPrompt}\n\nCâu hỏi của người dùng: {message}\n\nHãy trả lời ngắn gọn, thân thiện và hữu ích bằng tiếng Việt.";
            
            // Call Gemini API
            var aiResponse = await CallGeminiApiAsync(fullPrompt);
            
            return new AiChatResponseDto
            {
                Message = aiResponse,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat for user {UserId}", userId);
            return new AiChatResponseDto
            {
                Message = "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<AiInsightDto> GetDailyInsightAsync(long userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            
            // Get recent transactions
            var recentTransactions = await _context.Transactions
                .Where(t => t.Account.UserId == userId && t.TransactionDate >= weekAgo)
                .Include(t => t.Category)
                .ToListAsync();
            
            var weekExpense = recentTransactions
                .Where(t => t.TransactionType == 2)
                .Sum(t => t.Amount);
            
            // Get category breakdown
            var topCategory = recentTransactions
                .Where(t => t.TransactionType == 2)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();
            
            // Get previous week for comparison
            var twoWeeksAgo = weekAgo.AddDays(-7);
            var previousWeekExpense = await _context.Transactions
                .Where(t => t.Account.UserId == userId && 
                       t.TransactionDate >= twoWeeksAgo && 
                       t.TransactionDate < weekAgo &&
                       t.TransactionType == 2)
                .SumAsync(t => t.Amount);
            
            var changePercent = previousWeekExpense > 0 
                ? ((weekExpense - previousWeekExpense) / previousWeekExpense * 100) 
                : 0;
            
            string insight;
            string type;
            
            if (changePercent > 40)
            {
                insight = $"⚠️ Tuần này bạn đã chi {weekExpense:N0}đ cho {topCategory?.Category ?? "các mục"}, cao hơn {changePercent:F0}% so với tuần trước. Bạn có muốn đặt hạn mức cho danh mục này không?";
                type = "warning";
            }
            else if (changePercent < -20)
            {
                insight = $"🎉 Tuyệt vời! Chi tiêu tuần này giảm {Math.Abs(changePercent):F0}% so với tuần trước. Bạn đang tiết kiệm rất tốt!";
                type = "success";
            }
            else
            {
                insight = $"💡 Tuần này bạn chi {weekExpense:N0}đ. Danh mục chi nhiều nhất là {topCategory?.Category ?? "chưa xác định"} ({topCategory?.Total:N0}đ).";
                type = "info";
            }
            
            return new AiInsightDto
            {
                Title = "Góc nhìn AI hôm nay",
                Insight = insight,
                Type = type,
                ActionText = "Xem chi tiết",
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily insight for user {UserId}", userId);
            return new AiInsightDto
            {
                Title = "Góc nhìn AI",
                Insight = "Hãy thêm giao dịch để nhận được phân tích từ AI.",
                Type = "info",
                ActionText = "Thêm giao dịch",
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<AiCashflowForecastDto> GetCashflowForecastAsync(long userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var daysInMonth = (monthEnd - monthStart).Days + 1;
            var daysElapsed = (today - monthStart).Days + 1;
            var daysRemaining = (monthEnd - today).Days;
            
            // Get current month transactions
            var monthTransactions = await _context.Transactions
                .Where(t => t.Account.UserId == userId && 
                       t.TransactionDate >= monthStart && 
                       t.TransactionDate <= monthEnd)
                .ToListAsync();
            
            var monthIncome = monthTransactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var monthExpense = monthTransactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
            
            // Get current balance
            var currentBalance = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentBalance);
            
            // Calculate daily burn rate
            var dailyBurnRate = daysElapsed > 0 ? monthExpense / daysElapsed : 0;
            
            // Get upcoming scheduled transactions
            var upcomingExpenses = await _context.ScheduledTransactions
                .Where(st => st.UserId == userId && 
                       st.NextRunDate >= DateOnly.FromDateTime(today) && 
                       st.NextRunDate <= DateOnly.FromDateTime(monthEnd) &&
                       st.TransactionType == 2)
                .SumAsync(st => st.Amount);
            
            // Forecast end of month balance
            var projectedExpense = (dailyBurnRate * daysRemaining) + upcomingExpenses;
            var projectedBalance = currentBalance - projectedExpense;
            
            string forecast;
            string severity;
            
            if (projectedBalance < 0)
            {
                var daysUntilZero = currentBalance / dailyBurnRate;
                var zeroDate = today.AddDays((int)daysUntilZero);
                
                forecast = $"⚠️ Cảnh báo: Với tốc độ chi tiêu hiện tại ({dailyBurnRate:N0}đ/ngày), bạn sẽ âm tiền vào ngày {zeroDate:dd/MM}. Bạn cần giảm chi tiêu xuống còn {(currentBalance / daysRemaining):N0}đ/ngày để trụ đến cuối tháng.";
                severity = "danger";
            }
            else if (projectedBalance < currentBalance * 0.2m)
            {
                forecast = $"⚡ Lưu ý: Số dư dự kiến cuối tháng chỉ còn {projectedBalance:N0}đ. Hãy cân nhắc chi tiêu thận trọng hơn.";
                severity = "warning";
            }
            else
            {
                forecast = $"✅ Tốt! Dự kiến cuối tháng bạn sẽ còn {projectedBalance:N0}đ. Tiếp tục duy trì!";
                severity = "success";
            }
            
            return new AiCashflowForecastDto
            {
                CurrentBalance = currentBalance,
                ProjectedBalance = projectedBalance,
                DailyBurnRate = dailyBurnRate,
                DaysRemaining = daysRemaining,
                Forecast = forecast,
                Severity = severity,
                MonthIncome = monthIncome,
                MonthExpense = monthExpense,
                UpcomingExpenses = upcomingExpenses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cashflow forecast for user {UserId}", userId);
            throw;
        }
    }

    public async Task MarkSuggestionAsReadAsync(long userId, long suggestionId)
    {
        var suggestion = await _context.AiSuggestions
            .FirstOrDefaultAsync(s => s.Id == suggestionId && s.UserId == userId);
        
        if (suggestion != null)
        {
            suggestion.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    private async Task<string> CallGeminiApiAsync(string prompt)
    {
        try
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            var model = _configuration["GeminiAI:Model"] ?? "gemini-pro";
            var baseUrl = _configuration["GeminiAI:BaseUrl"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                // Fallback to rule-based response
                return GenerateRuleBasedResponse(prompt);
            }
            
            var url = $"{baseUrl}/models/{model}:generateContent?key={apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API returned {StatusCode}", response.StatusCode);
                return GenerateRuleBasedResponse(prompt);
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var text = result
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            
            return text ?? "Xin lỗi, tôi không thể tạo câu trả lời lúc này.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return GenerateRuleBasedResponse(prompt);
        }
    }

    private string GenerateRuleBasedResponse(string prompt)
    {
        // Simple rule-based fallback
        var lowerPrompt = prompt.ToLower();
        
        if (lowerPrompt.Contains("grab") || lowerPrompt.Contains("xe"))
        {
            return "Để tiết kiệm chi phí đi lại, bạn có thể: 1) Sử dụng xe bus/xe công cộng cho các chuyến đi ngắn, 2) Đi chung xe với đồng nghiệp, 3) Cân nhắc mua xe máy nếu đi lại thường xuyên.";
        }
        else if (lowerPrompt.Contains("tiết kiệm") || lowerPrompt.Contains("save"))
        {
            return "Để tiết kiệm hiệu quả: 1) Áp dụng quy tắc 50/30/20 (50% nhu cầu thiết yếu, 30% mong muốn, 20% tiết kiệm), 2) Theo dõi chi tiêu hàng ngày, 3) Đặt mục tiêu tiết kiệm cụ thể, 4) Tự động chuyển tiền vào tài khoản tiết kiệm mỗi tháng.";
        }
        else if (lowerPrompt.Contains("chi tiêu") || lowerPrompt.Contains("spending"))
        {
            return "Tôi đã phân tích chi tiêu của bạn. Hãy xem lại các danh mục chi tiêu lớn nhất và tìm cách tối ưu. Bạn có thể đặt ngân sách cho từng danh mục để kiểm soát tốt hơn.";
        }
        else if (lowerPrompt.Contains("tháng sau") || lowerPrompt.Contains("next month"))
        {
            return "Để chuẩn bị cho tháng sau: 1) Xem lại các khoản chi cố định (tiền nhà, điện nước, internet), 2) Lập kế hoạch chi tiêu dựa trên thu nhập dự kiến, 3) Dành 10-20% thu nhập cho quỹ khẩn cấp.";
        }
        else
        {
            return "Tôi là trợ lý tài chính AI của bạn. Tôi có thể giúp bạn: phân tích chi tiêu, đưa ra lời khuyên tiết kiệm, dự báo dòng tiền, và trả lời các câu hỏi về tài chính cá nhân. Hãy hỏi tôi bất cứ điều gì!";
        }
    }

    private async Task<Dictionary<string, object>> GetUserFinancialDataAsync(long userId)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-3);
        
        var transactions = await _context.Transactions
            .Where(t => t.Account.UserId == userId && t.TransactionDate >= startDate)
            .Include(t => t.Category)
            .Select(t => new
            {
                t.Amount,
                t.TransactionType,
                CategoryName = t.Category.Name,
                t.TransactionDate
            })
            .ToListAsync();
        
        var totalIncome = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
        
        var categoryBreakdown = transactions
            .Where(t => t.TransactionType == 2)
            .GroupBy(t => t.CategoryName)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();
        
        return new Dictionary<string, object>
        {
            ["totalIncome"] = totalIncome,
            ["totalExpense"] = totalExpense,
            ["categoryBreakdown"] = categoryBreakdown,
            ["transactionCount"] = transactions.Count
        };
    }

    private string BuildSystemPrompt(Dictionary<string, object> financialData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là một chuyên gia tài chính cá nhân thân thiện và hữu ích.");
        sb.AppendLine("Dữ liệu tài chính của người dùng (3 tháng gần nhất):");
        sb.AppendLine($"- Tổng thu nhập: {financialData["totalIncome"]:N0}đ");
        sb.AppendLine($"- Tổng chi tiêu: {financialData["totalExpense"]:N0}đ");
        sb.AppendLine($"- Số giao dịch: {financialData["transactionCount"]}");
        
        return sb.ToString();
    }
}
