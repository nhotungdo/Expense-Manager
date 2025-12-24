using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MoneyTrackerApp.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly ExpenseManagerContext _db;
        private readonly MoneyTrackerApp.Services.JwtTokenService _jwtService;
        private readonly MoneyTrackerApp.Services.ISessionService _sessionService;
        private readonly MoneyTrackerApp.Services.IMultiAccountService _multiAccountService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ExpenseManagerContext db, MoneyTrackerApp.Services.JwtTokenService jwtService, MoneyTrackerApp.Services.ISessionService sessionService, MoneyTrackerApp.Services.IMultiAccountService multiAccountService, ILogger<LoginModel> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _sessionService = sessionService;
            _sessionService = sessionService;
            _multiAccountService = multiAccountService;
            _logger = logger;
        }

        public IActionResult OnGet(string? action)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                 if (action != "add_account")
                 {
                     return Redirect("/home");
                 }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Redirect("/Auth/Login?error=missing_fields");
            }

            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || !user.Enabled)
                {
                    // Don't reveal user existence, but for debugging/flow we redirect with generic error
                    return Redirect("/Auth/Login?error=invalid_credentials");
                }

                // Check Lockout
                if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                {
                    var minutes = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return Redirect($"/Auth/Login?error=locked_out&details={minutes}_minutes");
                }

                // Verify Password
                if (!VerifyPassword(password, user.PasswordHash ?? string.Empty))
                {
                    user.AccessFailedCount++;
                    if (user.AccessFailedCount >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                        user.AccessFailedCount = 0;
                         _logger.LogWarning($"User {email} locked out due to too many failed attempts.");
                    }
                    await _db.SaveChangesAsync();
                    
                    return Redirect("/Auth/Login?error=invalid_credentials");
                }

                // Reset Lockout on success
                if (user.AccessFailedCount > 0)
                {
                    user.AccessFailedCount = 0;
                }
                user.LastLogin = DateTime.UtcNow;
                
                // Track Session
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var session = await _sessionService.CreateSessionAsync(user.Id, userAgent, ipAddress);

                // Issue Tokens
                var (accessToken, refreshToken) = await _jwtService.IssueAsync(user, session.Id);
                
                // Save DB changes (LastLogin, etc)
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User {email} logged in successfully via Page Model");

                // Set Cookie
                Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(60)
                });

                // [MultiAccount] Track Session
                _multiAccountService.AddSessionToCookie(session.Id);

                // Redirect
                var baseUrl = user.OnboardingCompleted ? "/home" : "/Onboarding/Welcome";
                if (user.Role == "Admin")
                {
                    baseUrl = "/Admin/Dashboard";
                }
                
                // We pass tokens in URL for client-side storage if needed, though Cookie is primary
                var redirectUrl = $"{baseUrl}?accessToken={Uri.EscapeDataString(accessToken)}&refreshToken={Uri.EscapeDataString(refreshToken)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return Redirect("/Auth/Login?error=server_error");
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            try 
            {
                // Use BCrypt to match AuthController
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch 
            {
                return false;
            }
        }
    }
}