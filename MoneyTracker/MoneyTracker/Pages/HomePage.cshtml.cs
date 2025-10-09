using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    public class HomePageModel : PageModel
    {
        private readonly ILogger<HomePageModel> _logger;

        public HomePageModel(ILogger<HomePageModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If user is authenticated, redirect to dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Dashboard");
            }

            // Set page metadata
            ViewData["Title"] = "Money Tracker - Quản lý tài chính thông minh";
            ViewData["Description"] = "Ứng dụng quản lý tài chính cá nhân thông minh với AI";
            ViewData["Keywords"] = "homepage, landing page, tài chính, quản lý tiền bạc, AI";

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
