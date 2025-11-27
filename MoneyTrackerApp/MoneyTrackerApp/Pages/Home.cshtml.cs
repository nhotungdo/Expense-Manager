using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace MoneyTrackerApp.Pages
{
    public class HomeModel : PageModel
    {
        private readonly ExpenseManagerContext _db;
        private readonly ILogger<HomeModel> _logger;

        public string UserEmail { get; set; } = string.Empty;
        public long UserId { get; set; }

        public HomeModel(ExpenseManagerContext db, ILogger<HomeModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Get user from JWT token in Authorization header
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    // If no auth header, try to get from cookie or redirect
                    var token = HttpContext.Request.Cookies["accessToken"];
                    if (string.IsNullOrEmpty(token))
                    {
                        Redirect("/Auth/Login");
                        return;
                    }
                    authHeader = $"Bearer {token}";
                }

                // Extract user info from token
                var tokenString = authHeader.Substring("Bearer ".Length);
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(tokenString))
                {
                    var token = handler.ReadJwtToken(tokenString);
                    var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                    var emailClaim = token.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);

                    if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                    {
                        // Verify user exists
                        var user = await _db.Users.FindAsync(userId);
                        if (user == null || !user.Enabled)
                        {
                            Redirect("/Auth/Login");
                            return;
                        }
                        UserId = userId;
                        UserEmail = emailClaim?.Value ?? string.Empty;
                    }
                    else
                    {
                        Redirect("/Auth/Login");
                        return;
                    }
                }
                else
                {
                    Redirect("/Auth/Login");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Home page load error: {ex.Message}");
                Redirect("/Auth/Login");
            }
        }
    }
}
