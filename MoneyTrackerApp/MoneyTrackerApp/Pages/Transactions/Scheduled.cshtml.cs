using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Transactions
{
    public class ScheduledModel : PageModel
    {
        private readonly IScheduledTransactionService _scheduledService;

        public ScheduledModel(IScheduledTransactionService scheduledService)
        {
            _scheduledService = scheduledService;
        }

        public List<ScheduledTransactionResponseDto> ScheduledTransactions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage("/Auth/Login");

            ScheduledTransactions = await _scheduledService.GetUserScheduledTransactionsAsync(userId, activeOnly: false);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdStr, out long userId))
            {
                await _scheduledService.DeleteScheduledTransactionAsync(id, userId);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(long id, bool isActive)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdStr, out long userId))
            {
                await _scheduledService.ToggleScheduledTransactionAsync(id, userId, isActive);
            }
            return RedirectToPage();
        }
    }
}
