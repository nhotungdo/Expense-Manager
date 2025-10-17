using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class AdminSystemSettingsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AdminSystemSettingsModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<SystemSettingItem> Items { get; set; } = new();

    [BindProperty]
    public long Id { get; set; }
    [BindProperty]
    public UpsertInput Upsert { get; set; } = new();

    public class SystemSettingItem
    {
        public long Id { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string SettingType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class UpsertInput
    {
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string SettingType { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var list = await client.GetFromJsonAsync<List<SystemSettingItem>>("/api/admin/system-settings");
        Items = list ?? new List<SystemSettingItem>();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (string.Equals(action, "create", StringComparison.Ordinal))
        {
            await client.PostAsJsonAsync("/api/admin/system-settings", Upsert);
        }
        else if (string.Equals(action, "update", StringComparison.Ordinal))
        {
            await client.PutAsJsonAsync($"/api/admin/system-settings/{Id}", Upsert);
        }
        else if (string.Equals(action, "delete", StringComparison.Ordinal))
        {
            await client.DeleteAsync($"/api/admin/system-settings/{Id}");
        }
        return RedirectToPage("/Admin/SystemSettings");
    }
}


