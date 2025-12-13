using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Savings
{
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public IList<SavingsGoal> SavingsGoals { get; set; } = default!;
        public decimal TotalSaved { get; set; }
        public decimal TotalTarget { get; set; }
        public int CompletedGoalsCount { get; set; }
        public int TotalGoalsCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            SavingsGoals = await _context.SavingsGoals
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            TotalSaved = SavingsGoals.Sum(s => s.CurrentAmount);
            TotalTarget = SavingsGoals.Sum(s => s.TargetAmount);
            TotalGoalsCount = SavingsGoals.Count;
            CompletedGoalsCount = SavingsGoals.Count(s => s.CurrentAmount >= s.TargetAmount);

            return Page();
        }
    }
}
