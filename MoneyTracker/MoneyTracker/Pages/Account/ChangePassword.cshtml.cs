using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

[MoneyTracker.Filters.RequireJwtCookie]
public class ChangePasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public ChangePasswordModel(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    public class ChangePasswordInput
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var token = Request.Cookies["jwt"] ?? string.Empty;
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await client.PutAsJsonAsync("/api/users/me/password", Input);
            if (!resp.IsSuccessStatusCode)
            {
                Error = "Unable to change password";
                return Page();
            }
            Success = "Password updated";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        return Page();
    }
}


