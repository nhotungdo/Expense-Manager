using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ILogger<DashboardModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Set page metadata
            ViewData["Title"] = "Dashboard - MoneyTracker";
            ViewData["Description"] = "Dashboard tổng quan tài chính cá nhân";
            ViewData["Keywords"] = "dashboard, tài chính, tổng quan, thu chi";

            // Set user data for the view
            ViewData["UserName"] = GetCurrentUserName();
            ViewData["UserEmail"] = GetCurrentUserEmail();

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