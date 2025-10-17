using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class AdminUsersModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AdminUsersModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<UserItem> Items { get; set; } = new();

    [BindProperty]
    public long Id { get; set; }

    public class UserItem
    {
        public long Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public bool Enabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await client.GetFromJsonAsync<PagedResponse<UserItem>>("/api/admin/users?page=1&pageSize=100");
        Items = res?.items ?? new List<UserItem>();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (string.Equals(action, "lock", StringComparison.Ordinal))
        {
            await client.PutAsJsonAsync($"/api/admin/users/{Id}", new { Enabled = (bool?)null, Lock = true });
        }
        else if (string.Equals(action, "unlock", StringComparison.Ordinal))
        {
            await client.PutAsJsonAsync($"/api/admin/users/{Id}", new { Enabled = (bool?)null, Lock = false });
        }
        return RedirectToPage("/Admin/Users");
    }

    private class PagedResponse<T> { public int total { get; set; } public int page { get; set; } public int pageSize { get; set; } public List<T> items { get; set; } = new(); }
}


