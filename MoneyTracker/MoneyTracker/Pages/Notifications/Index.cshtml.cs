using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class NotificationsIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public NotificationsIndexModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<NotificationItem> Items { get; set; } = new();

    public class NotificationItem
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await client.GetFromJsonAsync<PagedResponse<NotificationItem>>("/api/notifications?page=1&pageSize=50");
        Items = res?.items ?? new List<NotificationItem>();
    }

    public async Task<IActionResult> OnPostAsync(string action, long? id)
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (string.Equals(action, "readAll", StringComparison.Ordinal))
        {
            await client.PutAsync("/api/notifications/read-all", null);
        }
        else if (string.Equals(action, "readOne", StringComparison.Ordinal) && id.HasValue)
        {
            await client.PutAsync($"/api/notifications/{id.Value}/read", null);
        }
        return RedirectToPage("/Notifications/Index");
    }

    private class PagedResponse<T> { public int total { get; set; } public int page { get; set; } public int pageSize { get; set; } public List<T> items { get; set; } = new(); }
}


