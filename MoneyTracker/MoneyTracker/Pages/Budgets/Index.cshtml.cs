using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

[MoneyTracker.Filters.RequireJwtCookie]
public class BudgetsIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public BudgetsIndexModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public List<BudgetItem> Items { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
    public Dictionary<long, ProgressItem> ProgressMap { get; set; } = new();

    [BindProperty]
    public CreateInput Create { get; set; } = new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30), Period = 30 };

    public class BudgetItem
    {
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public int Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class CategoryItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class CreateInput
    {
        public long? CategoryId { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [Range(1, 400)]
        public int Period { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
    public class ProgressItem
    {
        public decimal spent { get; set; }
        public decimal budget { get; set; }
        public double percent { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var cats = await client.GetFromJsonAsync<List<CategoryItem>>("/api/categories");
        Categories = cats ?? new List<CategoryItem>();

        var list = await client.GetFromJsonAsync<List<BudgetItem>>("/api/budgets");
        Items = list ?? new List<BudgetItem>();

        foreach (var b in Items)
        {
            var p = await client.GetFromJsonAsync<ProgressItem>($"/api/budgets/{b.Id}/progress");
            if (p != null) ProgressMap[b.Id] = p;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGet();
            return Page();
        }
        var token = Request.Cookies["jwt"] ?? string.Empty;
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.PostAsJsonAsync("/api/budgets", Create);
        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Create budget failed";
            return RedirectToPage("/Budgets/Index");
        }
        TempData["Success"] = "Budget created";
        return RedirectToPage("/Budgets/Index");
    }
}


