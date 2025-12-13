using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Linq;

namespace MoneyTrackerApp.Services
{
    public class AiAdvisorService : IAiAdvisorService
    {
        private readonly ExpenseManagerContext _context;

        public AiAdvisorService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<AiSuggestion> GenerateAdviceAsync(long userId)
        {
            // Placeholder logic
            var suggestion = new AiSuggestion
            {
                UserId = userId,
                Suggestion = "Consider saving more this month based on your spending habits.",
                SuggestionType = "Savings",
                CreatedAt = DateTime.UtcNow
            };

            _context.AiSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            return suggestion;
        }

        public async Task<IEnumerable<AiSuggestion>> GetHistoryAsync(long userId)
        {
            return await _context.AiSuggestions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
