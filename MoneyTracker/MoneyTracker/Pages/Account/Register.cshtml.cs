using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string? Error { get; set; }
    public string? Token { get; set; }

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public class RegisterInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
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
            var response = await client.PostAsJsonAsync("/api/auth/register", new { Email = Input.Email, Password = Input.Password, FirstName = Input.FirstName, LastName = Input.LastName });
            if (!response.IsSuccessStatusCode)
            {
                Error = "Registration failed";
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


