using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    public class BudgetsModel : PageModel
    {
        private readonly ILogger<BudgetsModel> _logger;

        public BudgetsModel(ILogger<BudgetsModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access Budgets, redirecting to Login");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("Budgets page accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "Ngân Sách - MoneyTracker";
            ViewData["Description"] = "Quản lý ngân sách và theo dõi chi tiêu";
            ViewData["Keywords"] = "ngân sách, budget, quản lý tài chính, chi tiêu";

            return Page();
        }

        public string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        public string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        public string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }

        public string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";
        }

        public bool IsAdmin()
        {
            return User.IsInRole("ADMIN");
        }
    }
}
