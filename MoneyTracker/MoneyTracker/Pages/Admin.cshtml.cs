using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ILogger<AdminModel> _logger;

        public AdminModel(ILogger<AdminModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access Admin, redirecting to Login");
                return RedirectToPage("/Login");
            }

            // Check if user is admin
            if (!User.IsInRole("ADMIN"))
            {
                _logger.LogWarning("Non-admin user {UserId} tried to access Admin page", GetCurrentUserId());
                return RedirectToPage("/HomePage");
            }

            _logger.LogInformation("Admin page accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "Quản Trị - MoneyTracker";
            ViewData["Description"] = "Quản lý người dùng và giám sát hệ thống";
            ViewData["Keywords"] = "admin, quản trị, hệ thống, người dùng";

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