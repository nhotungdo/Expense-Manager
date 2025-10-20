using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Savings;

[Authorize]
public class GoalsModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public GoalsModel(ExpenseManagerContext db) { _db = db; }

    public List<SavingsGoal> Goals { get; set; } = new();
    public Dictionary<long, string> Suggestions { get; set; } = new();
    public List<string> SmartSuggestions { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);

        Goals = await _db.SavingsGoals
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.TargetDate)
            .ToListAsync();

        // Generate suggestions for each goal
        foreach (var goal in Goals)
        {
            if (goal.TargetDate.HasValue)
            {
                var targetDateTime = goal.TargetDate.Value.ToDateTime(new TimeOnly(0));
                var daysRemaining = (targetDateTime - DateTime.Today).Days;
                var amountNeeded = goal.TargetAmount - goal.CurrentAmount;

                if (daysRemaining > 0 && amountNeeded > 0)
                {
                    var dailyAmount = amountNeeded / daysRemaining;
                    var monthlyAmount = dailyAmount * 30;

                    if (monthlyAmount > 0)
                    {
                        Suggestions[goal.Id] = $"Save ${monthlyAmount:N2} per month to reach your goal on time";
                    }
                }
            }
        }

        // Generate smart suggestions based on spending patterns
        await GenerateSmartSuggestions(userId);
    }

    private async Task GenerateSmartSuggestions(long userId)
    {
        var monthlyExpenses = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 0 &&
                       t.TransactionDate >= DateTime.Today.AddDays(-30))
            .SumAsync(t => t.Amount);

        var monthlyIncome = await _db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == 1 &&
                       t.TransactionDate >= DateTime.Today.AddDays(-30))
            .SumAsync(t => t.Amount);

        if (monthlyIncome > 0)
        {
            var savingsRate = (monthlyIncome - monthlyExpenses) / monthlyIncome;

            if (savingsRate < 0.1m)
            {
                SmartSuggestions.Add("Consider increasing your savings rate to at least 10% of income");
            }

            if (monthlyExpenses > 0)
            {
                var avgDailyExpense = monthlyExpenses / 30;
                SmartSuggestions.Add($"Reducing daily expenses by ${avgDailyExpense * 0.1m:N2} could save ${avgDailyExpense * 0.1m * 30:N2} per month");
            }
        }

        var totalGoals = Goals.Sum(g => g.TargetAmount - g.CurrentAmount);
        if (totalGoals > 0)
        {
            SmartSuggestions.Add($"You have ${totalGoals:N2} remaining across all goals. Consider prioritizing by target date.");
        }
    }
}
