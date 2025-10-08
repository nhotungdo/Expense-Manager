using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    public class AIModel : PageModel
    {
        private readonly ILogger<AIModel> _logger;

        public AIModel(ILogger<AIModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access AI, redirecting to Login");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("AI page accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "AI Gợi Ý - MoneyTracker";
            ViewData["Description"] = "Gợi ý thông minh từ AI";
            ViewData["Keywords"] = "AI, gợi ý, thông minh, tài chính";

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