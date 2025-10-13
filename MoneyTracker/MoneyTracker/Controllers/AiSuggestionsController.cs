using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;
using MoneyTracker.Services;
using System.Security.Claims;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiSuggestionsController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly IGeminiSuggestionService _geminiService;
        private readonly ILogger<AiSuggestionsController> _logger;

        public AiSuggestionsController(
            ExpenseManagerContext context,
            IGeminiSuggestionService geminiService,
            ILogger<AiSuggestionsController> logger)
        {
            _context = context;
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSuggestions()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                // Fetch the last 30 days of transactions for the user
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var recentTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                               t.TransactionDate >= DateOnly.FromDateTime(thirtyDaysAgo))
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                if (!recentTransactions.Any())
                {
                    return BadRequest(new { message = "Không có giao dịch nào trong 30 ngày gần đây để tạo gợi ý." });
                }

                // Get AI suggestion from Gemini
                var suggestionText = await _geminiService.GetFinancialSuggestionAsync(recentTransactions);

                // Save the suggestion to the database
                var aiSuggestion = new AiSuggestion
                {
                    UserId = userId.Value,
                    Suggestion = suggestionText,
                    SuggestionType = "Financial Advice",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AiSuggestions.Add(aiSuggestion);
                await _context.SaveChangesAsync();

                _logger.LogInformation("AI suggestion generated and saved for user {UserId}", userId);

                // Return the created suggestion
                var response = new AiSuggestionDto
                {
                    Id = aiSuggestion.Id,
                    UserId = aiSuggestion.UserId,
                    Suggestion = aiSuggestion.Suggestion,
                    SuggestionType = aiSuggestion.SuggestionType,
                    IsRead = aiSuggestion.IsRead,
                    CreatedAt = aiSuggestion.CreatedAt ?? DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI suggestions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var suggestions = await _context.AiSuggestions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .Select(s => new AiSuggestionDto
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        Suggestion = s.Suggestion,
                        SuggestionType = s.SuggestionType,
                        IsRead = s.IsRead,
                        CreatedAt = s.CreatedAt ?? DateTime.UtcNow
                    })
                    .ToListAsync();

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AI suggestions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var suggestion = await _context.AiSuggestions
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

                if (suggestion == null)
                {
                    return NotFound();
                }

                suggestion.IsRead = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Suggestion marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking suggestion as read");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSuggestion(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }

                var suggestion = await _context.AiSuggestions
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

                if (suggestion == null)
                {
                    return NotFound();
                }

                _context.AiSuggestions.Remove(suggestion);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Suggestion deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting suggestion");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
