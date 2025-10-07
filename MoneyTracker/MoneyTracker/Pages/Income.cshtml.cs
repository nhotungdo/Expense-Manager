using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTracker.Pages
{
    [Authorize]
    public class IncomeModel : PageModel
    {
        private readonly ILogger<IncomeModel> _logger;

        public IncomeModel(ILogger<IncomeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Income page initialization
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
    }
}
