using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin.Settings
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ISystemSettingsService _settingsService;

        public IndexModel(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public List<SystemSettingDto> Settings { get; set; } = new();

        [BindProperty]
        public UpdateSystemSettingDto UpdateSetting { get; set; } = new();

        public async Task OnGetAsync()
        {
            Settings = await _settingsService.GetAllSettingsAsync();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string key)
        {
            await _settingsService.UpdateSettingAsync(key, UpdateSetting);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMaintenanceModeAsync(bool enabled)
        {
            await _settingsService.SetMaintenanceModeAsync(enabled);
            return RedirectToPage();
        }
    }
}
