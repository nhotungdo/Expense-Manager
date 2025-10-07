using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker.Pages
{
    public class HomePageModel : PageModel
    {
        private readonly ILogger<HomePageModel> _logger;

        public HomePageModel(ILogger<HomePageModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("HomePage accessed at {Time}", DateTime.UtcNow);

            // Set page metadata
            ViewData["Title"] = "MoneyTracker - Quản lý tài chính thông minh";
            ViewData["Description"] = "Giải pháp quản lý tài chính cá nhân toàn diện và thông minh. Kiểm soát chi tiêu, tiết kiệm tiền và đạt được mục tiêu tài chính của bạn.";
            ViewData["Keywords"] = "quản lý tài chính, chi tiêu, tiết kiệm, budget, money tracker, tài chính cá nhân";
        }

        public IActionResult OnGetFeatures()
        {
            return RedirectToPage("/HomePage", null, "features");
        }

        public IActionResult OnGetAbout()
        {
            return RedirectToPage("/HomePage", null, "about");
        }

        public IActionResult OnGetContact()
        {
            return RedirectToPage("/HomePage", null, "contact");
        }
    }
}


