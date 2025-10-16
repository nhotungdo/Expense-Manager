using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Services;

namespace MoneyTracker.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _db;
        private readonly IAiService _aiService;

        public decimal TotalIncomeThisMonth { get; private set; }
        public decimal TotalExpenseThisMonth { get; private set; }
        public List<string> AiSuggestions { get; private set; } = new();

        public IndexModel(ExpenseManagerContext db, IAiService aiService)
        {
            _db = db;
            _aiService = aiService;
        }

        public async Task OnGet()
        {
            var now = System.DateTime.UtcNow;
            var monthStart = new System.DateTime(now.Year, now.Month, 1);
            var monthStartDateOnly = System.DateOnly.FromDateTime(monthStart);
            var nowDateOnly = System.DateOnly.FromDateTime(now);

            TotalIncomeThisMonth = await _db.Incomes
                .Where(i => i.IncomeDate >= monthStartDateOnly && i.IncomeDate <= nowDateOnly)
                .SumAsync(i => (decimal?)i.Amount) ?? 0m;

            TotalExpenseThisMonth = await _db.Expenses
                .Where(e => e.ExpenseDate >= monthStartDateOnly && e.ExpenseDate <= nowDateOnly)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var last30 = await _db.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(30)
                .Select(t => new AiTransactionInput { Date = t.TransactionDate, CategoryId = t.CategoryId, Amount = t.Amount })
                .ToListAsync();

            AiSuggestions = await _aiService.GetSuggestionsAsync(last30);
        }
    }
}

