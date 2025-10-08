using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTracker.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Login page accessed at {Time}", DateTime.UtcNow);

            // Check if user is already authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Authenticated user {UserId} accessed login page, redirecting to HomePage",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return RedirectToPage("/HomePage");
            }

            // Set page metadata
            ViewData["Title"] = "Đăng Nhập - MoneyTracker";
            ViewData["Description"] = "Đăng nhập vào hệ thống quản lý tài chính";
            ViewData["Keywords"] = "đăng nhập, login, tài chính, quản lý";

            return Page();
        }
    }
}