using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    public interface ISystemSettingsService
    {
        Task<List<SystemSettingDto>> GetAllSettingsAsync();
        Task<SystemSettingDto?> GetSettingAsync(string key);
        Task UpdateSettingAsync(string key, UpdateSystemSettingDto dto);
        Task<bool> IsMaintenanceModeAsync();
        Task SetMaintenanceModeAsync(bool enabled);
    }

    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ExpenseManagerContext _context;

        public SystemSettingsService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<List<SystemSettingDto>> GetAllSettingsAsync()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return settings.Select(s => new SystemSettingDto
            {
                Key = s.SettingKey,
                Value = s.SettingValue,
                Description = s.Description,
                Type = s.SettingType,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<SystemSettingDto?> GetSettingAsync(string key)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null) return null;

            return new SystemSettingDto
            {
                Key = setting.SettingKey,
                Value = setting.SettingValue,
                Description = setting.Description,
                Type = setting.SettingType,
                IsActive = setting.IsActive
            };
        }

        public async Task UpdateSettingAsync(string key, UpdateSystemSettingDto dto)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                // Create if not exists (optional, but good for dynamic settings)
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = dto.Value,
                    IsActive = dto.IsActive,
                    SettingType = "string", // Default
                    CreatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = dto.Value;
                setting.IsActive = dto.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMaintenanceModeAsync()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "MaintenanceMode");
            return setting != null && setting.IsActive && setting.SettingValue == "true";
        }

        public async Task SetMaintenanceModeAsync(bool enabled)
        {
            await UpdateSettingAsync("MaintenanceMode", new UpdateSystemSettingDto
            {
                Value = enabled ? "true" : "false",
                IsActive = true
            });
        }
    }
}
