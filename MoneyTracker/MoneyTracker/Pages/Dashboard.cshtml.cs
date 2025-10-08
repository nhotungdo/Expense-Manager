using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MoneyTracker.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ILogger<DashboardModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("Unauthenticated user tried to access Dashboard, redirecting to Login");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("Dashboard accessed by user {UserId} at {Time}",
                GetCurrentUserId(), DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "Dashboard - MoneyTracker";
            ViewData["Description"] = "Tổng quan tài chính cá nhân với thống kê chi tiết và biểu đồ trực quan";
            ViewData["Keywords"] = "dashboard, tài chính, thống kê, biểu đồ, thu chi";

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
