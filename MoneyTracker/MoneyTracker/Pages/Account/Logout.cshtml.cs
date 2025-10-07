using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTracker.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("User accessed logout page");
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("User confirmed logout");
            return RedirectToPage("/HomePage");
        }
    }
}
