using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    [Authorize]
    public class AIModel : PageModel
    {
        private readonly ILogger<AIModel> _logger;

        public AIModel(ILogger<AIModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Set page metadata
            ViewData["Title"] = "AI Suggestions - MoneyTracker";
            ViewData["Description"] = "AI-powered financial suggestions and insights";
            ViewData["Keywords"] = "AI, suggestions, financial advice, insights, money management";

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
    }
}