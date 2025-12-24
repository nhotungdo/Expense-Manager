using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin.Users
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUserManagementService _userService;

        public IndexModel(IUserManagementService userService)
        {
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public UserFilterDto Filter { get; set; } = new UserFilterDto { PageNumber = 1, PageSize = 10 };

        public List<AdminUserDto> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _userService.GetAllUsersAsync(Filter);
        }

        public async Task<IActionResult> OnPostLockAsync(long id, int duration = 1440)
        {
            await _userService.LockUserAsync(id, duration);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnlockAsync(long id)
        {
            await _userService.UnlockUserAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(long id)
        {
             await _userService.ResetPasswordAsync(id);
             TempData["Message"] = "Password reset email queued successfully.";
             return RedirectToPage();
        }
    }
}
