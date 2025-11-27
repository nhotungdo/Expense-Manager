using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/settings")]
    [Authorize]
    public class SystemSettingsController : ControllerBase
    {
        private readonly ISystemSettingsService _settingsService;

        public SystemSettingsController(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SystemSettingDto>>> GetAllSettings()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            return Ok(settings);
        }

        [HttpGet("{key}")]
        public async Task<ActionResult<SystemSettingDto>> GetSetting(string key)
        {
            var setting = await _settingsService.GetSettingAsync(key);
            if (setting == null) return NotFound();
            return Ok(setting);
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSystemSettingDto dto)
        {
            await _settingsService.UpdateSettingAsync(key, dto);
            return Ok();
        }

        [HttpGet("maintenance")]
        public async Task<ActionResult<bool>> GetMaintenanceMode()
        {
            return Ok(await _settingsService.IsMaintenanceModeAsync());
        }

        [HttpPost("maintenance")]
        public async Task<IActionResult> SetMaintenanceMode([FromBody] bool enabled)
        {
            await _settingsService.SetMaintenanceModeAsync(enabled);
            return Ok();
        }
    }
}
