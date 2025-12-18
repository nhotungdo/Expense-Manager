using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;
using System.Linq;

namespace MoneyTrackerApp.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IGeminiAnalysisService _geminiService;
        private readonly ExpenseManagerContext _context;

        public DashboardModel(IGeminiAnalysisService geminiService, ExpenseManagerContext context)
        {
            _geminiService = geminiService;
            _context = context;
        }

        public AiSuggestion? LatestSuggestion { get; set; }

        public async Task OnGetAsync()
        {
            var userId = GetUserId();
            
            // Prioritize Unread suggestions
            LatestSuggestion = await _context.AiSuggestions
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            // If no unread, show the latest one
            if (LatestSuggestion == null)
            {
                 LatestSuggestion = await _context.AiSuggestions
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IActionResult> OnPostGenerateAiInsightsAsync()
        {
            try 
            {
                var userId = GetUserId();
                // Get transactions last 30 days
                var date = DateTime.UtcNow.AddDays(-30);
                var transactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.TransactionDate >= date)
                    .OrderBy(t => t.TransactionDate)
                    .ToListAsync();
                    
                if (!transactions.Any())
                {
                     // Try to get ANY last transactions if no recent ones, just to show something?
                     // No, requirement says "khoảng thời gian (ví dụ: 30 ngày qua)". I'll stick to 30 days.
                     // But for demo purposes, if the user has no data, should returns message.
                     return new JsonResult(new { success = false, message = "Không có giao dịch nào trong 30 ngày qua để phân tích." });
                }
                
                // Map to DTO
                var dtos = transactions.Select(t => new TransactionAnalysisDto
                {
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Amount = t.Amount,
                    Currency = t.Currency ?? "VND",
                    Category = t.Category?.Name ?? "Khác",
                    Type = t.TransactionType == 1 ? "Thu" : (t.TransactionType == 2 ? "Chi" : "Chuyển tiền"),
                    Note = t.Note
                }).ToList();
                
                // Call Gemini
                var suggestionText = await _geminiService.AnalyzeTransactionsAsync(dtos);
                
                // Save to DB
                var suggestion = new AiSuggestion
                {
                    UserId = userId,
                    Suggestion = suggestionText,
                    SuggestionType = "Financial Analysis",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                
                _context.AiSuggestions.Add(suggestion);
                await _context.SaveChangesAsync();
                
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.ToString());
                 return new JsonResult(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        
        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
             var userId = GetUserId();
             var suggestion = await _context.AiSuggestions
                 .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
                 
             if (suggestion != null)
             {
                 suggestion.IsRead = true;
                 await _context.SaveChangesAsync();
                 return new OkResult();
             }
             return new NotFoundResult();
        }

        private long GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(idClaim, out var id)) return id;
            return 0;
        }
    }
}
