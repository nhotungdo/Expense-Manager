using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Settings;

[Authorize]
public class ThemeModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ThemeModel(ExpenseManagerContext db) { _db = db; }

    public bool IsDarkMode { get; set; }
    public string Language { get; set; } = "en";

    public async Task OnGetAsync()
    {
        // Load app-wide preferences from SystemSetting
        var darkModeSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "darkMode");
        IsDarkMode = darkModeSetting?.SettingValue == "true";

        var languageSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "language");
        Language = languageSetting?.SettingValue ?? "en";
    }

    public async Task<IActionResult> OnPostAsync(bool darkMode, string language)
    {
        // Save dark mode setting (app-wide)
        var darkModeSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "darkMode");
        if (darkModeSetting == null)
        {
            darkModeSetting = new SystemSetting
            {
                SettingKey = "darkMode",
                SettingValue = darkMode.ToString().ToLower()
            };
            _db.SystemSettings.Add(darkModeSetting);
        }
        else
        {
            darkModeSetting.SettingValue = darkMode.ToString().ToLower();
        }

        // Save language setting (app-wide)
        var languageSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "language");
        if (languageSetting == null)
        {
            languageSetting = new SystemSetting
            {
                SettingKey = "language",
                SettingValue = language
            };
            _db.SystemSettings.Add(languageSetting);
        }
        else
        {
            languageSetting.SettingValue = language;
        }

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
