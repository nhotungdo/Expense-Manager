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
public class TransactionsIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public TransactionsIndexModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public int Page { get; set; } = 1;
    public string? Error { get; set; }
    public List<TransactionItem> Items { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
    public Dictionary<long, string> CategoryNameMap { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterInput Filter { get; set; } = new();

    [BindProperty]
    public CreateInput Create { get; set; } = new() { TransactionDate = DateTime.Now };

    public class FilterInput
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Type { get; set; }
    }
    public class CreateInput
    {
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        public long CategoryId { get; set; }
        [Required]
        public string Type { get; set; } = "Expense";
        [Required]
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }
    public class TransactionItem
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public long CategoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }
    public class CategoryItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    private class PagedResponse<T> { public int total { get; set; } public int page { get; set; } public int pageSize { get; set; } public List<T> items { get; set; } = new(); }

    public async Task OnGetAsync(int page = 1)
    {
        Page = page;
        try
        {
            var token = Request.Cookies["jwt"] ?? string.Empty;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // categories
            var cats = await client.GetFromJsonAsync<List<CategoryItem>>("/api/categories");
            Categories = cats ?? new List<CategoryItem>();
            foreach (var c in Categories) CategoryNameMap[c.Id] = c.Name;

            // transactions
            var url = $"/api/transactions?page={Page}&pageSize=20";
            if (Filter.From.HasValue) url += $"&from={Filter.From:O}";
            if (Filter.To.HasValue) url += $"&to={Filter.To:O}";
            if (!string.IsNullOrEmpty(Filter.Type)) url += $"&type={Uri.EscapeDataString(Filter.Type)}";
            var res = await client.GetFromJsonAsync<PagedResponse<TransactionItem>>(url);
            Items = res?.items ?? new List<TransactionItem>();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }
        try
        {
            var token = Request.Cookies["jwt"] ?? string.Empty;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.PostAsJsonAsync("/api/transactions", Create);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Create transaction failed";
                return RedirectToPage("/Transactions/Index");
            }
            TempData["Success"] = "Transaction created";
            return RedirectToPage("/Transactions/Index");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }
    }
}


