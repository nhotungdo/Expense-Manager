using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class AdminAuditLogsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AdminAuditLogsModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<AuditItem> Items { get; set; } = new();
    public FilterInput Filter { get; set; } = new();

    public class AuditItem
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public long? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class FilterInput
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
    }

    public async Task OnGet(DateTime? from, DateTime? to, string? action, string? entityType)
    {
        Filter.From = from; Filter.To = to; Filter.Action = action; Filter.EntityType = entityType;
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = "/api/admin/audit-logs?page=1&pageSize=100";
        if (from.HasValue) url += $"&from={from:O}";
        if (to.HasValue) url += $"&to={to:O}";
        if (!string.IsNullOrEmpty(action)) url += $"&action={Uri.EscapeDataString(action)}";
        if (!string.IsNullOrEmpty(entityType)) url += $"&entityType={Uri.EscapeDataString(entityType)}";
        var res = await client.GetFromJsonAsync<PagedResponse<AuditItem>>(url);
        Items = res?.items ?? new List<AuditItem>();
    }

    private class PagedResponse<T> { public int total { get; set; } public int page { get; set; } public int pageSize { get; set; } public List<T> items { get; set; } = new(); }
}


