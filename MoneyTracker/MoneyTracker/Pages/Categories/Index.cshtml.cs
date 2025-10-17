using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class CategoriesIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CategoriesIndexModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<CategoryItem> Items { get; set; } = new();

    [BindProperty]
    public UpsertInput Upsert { get; set; } = new();
    [BindProperty]
    public long Id { get; set; }

    public class CategoryItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
    public class UpsertInput
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var list = await client.GetFromJsonAsync<List<CategoryItem>>("/api/categories");
        Items = list ?? new List<CategoryItem>();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (string.Equals(action, "create", StringComparison.Ordinal))
        {
            await client.PostAsJsonAsync("/api/categories", Upsert);
        }
        else if (string.Equals(action, "update", StringComparison.Ordinal))
        {
            await client.PutAsJsonAsync($"/api/categories/{Id}", new { Name = Upsert.Name, Type = Upsert.Type, Icon = Upsert.Icon, Color = Upsert.Color, IsActive = true });
        }
        else if (string.Equals(action, "delete", StringComparison.Ordinal))
        {
            await client.DeleteAsync($"/api/categories/{Id}");
        }

        return RedirectToPage("/Categories/Index");
    }
}


