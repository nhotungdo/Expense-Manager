using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string? Error { get; set; }
    public string? Token { get; set; }

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = Input.Email, Password = Input.Password });
            if (!response.IsSuccessStatusCode)
            {
                Error = "Invalid credentials";
                return Page();
            }
            var json = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Token = json?.token;
            if (!string.IsNullOrEmpty(Token))
            {
                Response.Cookies.Append("jwt", Token!, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        return Page();
    }

    private class TokenResponse { public string token { get; set; } = string.Empty; }
}


