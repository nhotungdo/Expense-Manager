using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Automation
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ExpenseManagerContext _context;

        public IndexModel(ExpenseManagerContext context)
        {
            _context = context;
        }

        public List<AutomationRule> Rules { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdStr, out long userId))
            {
                Rules = await _context.AutomationRules
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage();

            var rule = await _context.AutomationRules.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (rule != null)
            {
                _context.AutomationRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(long id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage();

            var rule = await _context.AutomationRules.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (rule != null)
            {
                rule.IsActive = !rule.IsActive;
                _context.AutomationRules.Update(rule);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
