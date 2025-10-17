using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[MoneyTracker.Filters.RequireJwtCookie]
public class ProfileModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string? Error { get; set; }
    public string? Success { get; set; }

    public ProfileModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public class ProfileInput
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Language { get; set; }
        public string? DefaultCurrency { get; set; }
        public string? Theme { get; set; }
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
    }

    public async Task OnGet()
    {
        try
        {
            var token = Request.Cookies["jwt"] ?? string.Empty;
            if (string.IsNullOrEmpty(token)) return;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var me = await client.GetFromJsonAsync<MeResponse>("/api/users/me");
            if (me != null)
            {
                Input.FirstName = me.FirstName;
                Input.LastName = me.LastName;
                Input.Address = me.Address;
                Input.Language = me.Language;
                Input.DefaultCurrency = me.DefaultCurrency;
                Input.Theme = me.Theme;
                Input.EmailNotifications = me.EmailNotifications;
                Input.PushNotifications = me.PushNotifications;
                Input.DateOfBirth = me.DateOfBirth;
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var token = Request.Cookies["jwt"] ?? string.Empty;
            if (string.IsNullOrEmpty(token))
            {
                Error = "Not logged in";
                return Page();
            }
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.PutAsJsonAsync("/api/users/me", Input);
            if (!resp.IsSuccessStatusCode)
            {
                Error = "Update failed";
                return Page();
            }
            Success = "Saved";
            if (!string.IsNullOrWhiteSpace(Input.Theme))
            {
                Response.Cookies.Append("theme", Input.Theme!, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
            }
            if (!string.IsNullOrWhiteSpace(Input.Language))
            {
                Response.Cookies.Append("lang", Input.Language!, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        return Page();
    }

    private class MeResponse
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string Language { get; set; } = string.Empty;
        public string DefaultCurrency { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
    }
}


