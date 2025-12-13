using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Wallets.Shared
{
    public class IndexModel : PageModel
    {
        private readonly ISharedAccountService _sharedAccountService;

        public IndexModel(ISharedAccountService sharedAccountService)
        {
            _sharedAccountService = sharedAccountService;
        }

        public List<SharedAccountListDto> SharedWallets { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage("/Auth/Login");

            SharedWallets = await _sharedAccountService.GetSharedAccountsForUserAsync(userId);
            return Page();
        }
    }
}
