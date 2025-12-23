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
        private readonly IAccountService _accountService;
        private readonly IFriendshipService _friendshipService;

        public IndexModel(
            ISharedAccountService sharedAccountService, 
            IAccountService accountService,
            IFriendshipService friendshipService)
        {
            _sharedAccountService = sharedAccountService;
            _accountService = accountService;
            _friendshipService = friendshipService;
        }

        public List<SharedAccountListDto> SharedWallets { get; set; } = new();
        public List<AccountResponseDto> MyWallets { get; set; } = new();
        public List<FriendshipDto> Friends { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) return RedirectToPage("/Auth/Login");

            SharedWallets = await _sharedAccountService.GetSharedAccountsForUserAsync(userId);
            MyWallets = await _accountService.GetUserAccountsAsync(userId);
            Friends = await _friendshipService.GetFriendsAsync(userId);
            
            return Page();
        }

        public async Task<IActionResult> OnPostShareAsync([FromBody] ShareAccountDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) 
                return new JsonResult(new { success = false, message = "Unauthorized" });

            try 
            {
                await _sharedAccountService.ShareAccountAsync(userId, dto);
                return new JsonResult(new { success = true });
            } 
            catch (Exception ex) 
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostLeaveAsync([FromBody] LeaveRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId)) 
                return new JsonResult(new { success = false, message = "Unauthorized" });

            try
            {
                var result = await _sharedAccountService.LeaveSharedAccountAsync(userId, req.SharedAccountId);
                if (result)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Shared wallet not found or could not be removed." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class LeaveRequest
        {
            public long SharedAccountId { get; set; }
        }
    }
}
