using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Text.Json;

namespace MoneyTrackerApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AiAdvisorController : ControllerBase
    {
        private readonly IGeminiAnalysisService _geminiService;
        private readonly ExpenseManagerContext _context;

        public AiAdvisorController(IGeminiAnalysisService geminiService, ExpenseManagerContext context)
        {
            _geminiService = geminiService;
            _context = context;
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                // 1. Fetch recent transactions (last 30 days)
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30);

                var transactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var analysisDtos = transactions.Select(t => new TransactionAnalysisDto
                {
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Amount = t.Amount,
                    Currency = "VND", // Assuming base currency for simplify
                    Category = t.Category?.Name ?? "Uncategorized",
                    Type = t.TransactionType == 1 ? "Income" : "Expense",
                    Note = t.Note
                }).ToList();

                var suggestions = new List<AiSuggestionResponse>();

                // 2. Try Gemini Analysis
                string geminiResponse = await _geminiService.AnalyzeTransactionsAsync(analysisDtos);

                // Check if Gemini worked or returned generic error/missing key
                bool geminiValid = !string.IsNullOrEmpty(geminiResponse) && 
                                   !geminiResponse.Contains("API Key is missing") && 
                                   !geminiResponse.StartsWith("Error:") && 
                                   !geminiResponse.StartsWith("AI Service Error:") &&
                                   geminiResponse != "No analysis returned from AI.";

                if (geminiValid)
                {
                    // Parse Gemini response which is expected to be unstructured text 1. 2. 3.
                    // We will wrap it in a single "General" suggestion or try to split it.
                    // For better UX, let's treat the whole response as a "Comprehensive Review".
                    suggestions.Add(new AiSuggestionResponse 
                    { 
                        SuggestionType = "Phân tích Tiên tiến (Gemini)", 
                        Suggestion = geminiResponse,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    // 3. Fallback: Rule-based Logic
                    suggestions.AddRange(GenerateRuleBasedSuggestions(transactions));
                }

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private List<AiSuggestionResponse> GenerateRuleBasedSuggestions(List<Transaction> transactions)
        {
            var list = new List<AiSuggestionResponse>();
            
            if (!transactions.Any())
            {
                list.Add(new AiSuggestionResponse 
                { 
                    SuggestionType = "Khởi đầu", 
                    Suggestion = "Bạn chưa có giao dịch nào trong 30 ngày qua. Hãy bắt đầu ghi chép chi tiêu để tôi có thể phân tích thói quen tài chính của bạn!",
                    CreatedAt = DateTime.UtcNow
                });
                return list;
            }

            // Calc totals
            var income = transactions.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
            var expense = transactions.Where(t => t.TransactionType == 2).Sum(t => t.Amount);
            var balance = income - expense;

            // 1. Balance Analysis
            if (balance > 0)
            {
                decimal saveRate = income > 0 ? (balance / income) * 100 : 0;
                list.Add(new AiSuggestionResponse 
                { 
                    SuggestionType = "Sức khỏe Tài chính (Tốt)", 
                    Suggestion = $"Bạn đang làm rất tốt! Tháng này bạn dư ra {balance:N0} VND ({saveRate:F1}% thu nhập). Hãy cân nhắc gửi tiết kiệm khoản này.",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (balance < 0)
            {
                 list.Add(new AiSuggestionResponse 
                { 
                    SuggestionType = "Cảnh báo Chi tiêu", 
                    Suggestion = $"Cẩn thận! Bạn đang chi tiêu vượt thu nhập {Math.Abs(balance):N0} VND. Hãy rà soát lại các khoản chi không cần thiết.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 2. Top Spending Category
            var expenseTrx = transactions.Where(t => t.TransactionType == 2).ToList();
            if (expenseTrx.Any())
            {
                var topCategory = expenseTrx
                    .GroupBy(t => t.Category?.Name ?? "Khác")
                    .Select(g => new { Name = g.Key, Amount = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Amount)
                    .FirstOrDefault();

                if (topCategory != null)
                {
                    decimal pct = expense > 0 ? (topCategory.Amount / expense) * 100 : 0;
                    list.Add(new AiSuggestionResponse
                    {
                        SuggestionType = "Phân tích Danh mục",
                        Suggestion = $"Khoản chi lớn nhất của bạn là cho '{topCategory.Name}' với {topCategory.Amount:N0} VND (chiếm {pct:F1}% tổng chi).",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // 3. General Tip (Random)
            var tips = new[] 
            {
                "Quy tắc 50/30/20: 50% cho nhu cầu, 30% cho mong muốn, 20% cho tiết kiệm/trả nợ.",
                "Hãy thử thanh toán bằng tiền mặt cho các khoản ăn uống để kiểm soát tốt nhận thức chi tiêu.",
                "Đừng quên lập ngân sách cho tháng sau trước khi tháng này kết thúc.",
                "Kiểm tra lại các khoản đăng ký định kỳ (Netflix, Spotify...) xem bạn có thực sự dùng hết không."
            };
            list.Add(new AiSuggestionResponse
            {
                SuggestionType = "Mẹo hay",
                Suggestion = tips[new Random().Next(tips.Length)],
                CreatedAt = DateTime.UtcNow
            });

            return list;
        }
    }

    public class AiSuggestionResponse
    {
        public string SuggestionType { get; set; }
        public string Suggestion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
