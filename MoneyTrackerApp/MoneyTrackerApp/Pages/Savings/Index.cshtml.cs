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

        [BindProperty]
        public SavingsGoalViewModel NewGoal { get; set; } = new();

        public class SavingsGoalViewModel
        {
            public string Name { get; set; } = string.Empty;
            public decimal TargetAmount { get; set; }
            public decimal InitialAmount { get; set; }
            public DateTime? TargetDate { get; set; }
            public string Icon { get; set; } = "fas fa-piggy-bank";
            public string Color { get; set; } = "#8b5cf6";
        }

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

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                // Reload data if invalid
                return await OnGetAsync();
            }

            var goal = new SavingsGoal
            {
                UserId = userId,
                Name = NewGoal.Name,
                TargetAmount = NewGoal.TargetAmount,
                CurrentAmount = NewGoal.InitialAmount,
                TargetDate = NewGoal.TargetDate.HasValue ? DateOnly.FromDateTime(NewGoal.TargetDate.Value) : null,
                Icon = NewGoal.Icon,
                Color = NewGoal.Color,
                Status = 1, // PlanningEnums.SavingsGoalStatus.Active (assuming 1 is Active)
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        [BindProperty]
        public long EditId { get; set; }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                return await OnGetAsync();
            }

            var goal = await _context.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == EditId && s.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Name = NewGoal.Name;
            goal.TargetAmount = NewGoal.TargetAmount;
            goal.CurrentAmount = NewGoal.InitialAmount; // In edit mode, this might overwrite progress if we aren't careful. Ideally we might want separate logic strictly for "Goal Settings" vs "Transaction Balance". 
            // However, typically "Edit Goal" allows changing current amount if it was manual entry. 
            // IMPROVEMENT: If we want to strictly track transactions, we shouldn't allow editing CurrentAmount directly here without a transaction. 
            // BUT for this simple app level, let's assume 'InitialAmount' field in the form acts as 'Current Balance' for the edit.
            
            goal.TargetDate = NewGoal.TargetDate.HasValue ? DateOnly.FromDateTime(NewGoal.TargetDate.Value) : null;
            goal.Icon = NewGoal.Icon;
            goal.Color = NewGoal.Color;
            goal.UpdatedAt = DateTime.UtcNow;

            _context.SavingsGoals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var goal = await _context.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal != null)
            {
                _context.SavingsGoals.Remove(goal);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
