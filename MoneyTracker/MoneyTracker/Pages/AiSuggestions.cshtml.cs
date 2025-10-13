using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    [Authorize]
    public class AiSuggestionsModel : PageModel
    {
        private readonly ILogger<AiSuggestionsModel> _logger;

        public AiSuggestionsModel(ILogger<AiSuggestionsModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access AI Suggestions, redirecting to Login");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("AI Suggestions page accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "AI Gợi ý Tài chính - MoneyTracker";
            ViewData["Description"] = "Nhận các gợi ý thông minh từ AI dựa trên phân tích thói quen chi tiêu";
            ViewData["Keywords"] = "AI, gợi ý, tài chính, phân tích, thông minh";

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
            return User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("UserName")?.Value ?? "User";
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