using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class DashboardIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public DashboardIndexModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public List<CategorySummaryItem> CategorySummary { get; set; } = new();

    public class CategorySummaryItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var overview = await client.GetFromJsonAsync<List<Dictionary<string, object>>>("/api/dashboard/overview");
        if (overview != null && overview.Count > 0)
        {
            var row = overview[0];
            if (row.TryGetValue("TotalIncome", out var ti)) TotalIncome = Convert.ToDecimal(ti);
            if (row.TryGetValue("TotalExpense", out var te)) TotalExpense = Convert.ToDecimal(te);
        }

        var summary = await client.GetFromJsonAsync<List<Dictionary<string, object>>>("/api/dashboard/category-summary");
        if (summary != null)
        {
            foreach (var s in summary)
            {
                var item = new CategorySummaryItem
                {
                    CategoryName = s.ContainsKey("CategoryName") ? Convert.ToString(s["CategoryName"]) ?? string.Empty : string.Empty,
                    TotalAmount = s.ContainsKey("TotalAmount") ? Convert.ToDecimal(s["TotalAmount"]) : 0m
                };
                CategorySummary.Add(item);
            }
        }
    }
}


