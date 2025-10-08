using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    public class TransactionsModel : PageModel
    {
        private readonly ILogger<TransactionsModel> _logger;

        public TransactionsModel(ILogger<TransactionsModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access Transactions, redirecting to Login");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("Transactions page accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "Giao Dịch - MoneyTracker";
            ViewData["Description"] = "Quản lý giao dịch thu chi";
            ViewData["Keywords"] = "giao dịch, transaction, thu chi, quản lý tài chính";

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
