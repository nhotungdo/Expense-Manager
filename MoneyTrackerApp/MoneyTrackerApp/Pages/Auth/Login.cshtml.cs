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
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ExpenseManagerContext db, MoneyTrackerApp.Services.JwtTokenService jwtService, ILogger<LoginModel> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Redirect("/Auth/Login?error=missing_fields");
            }

            try
            {
                // Find user by email
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Enabled);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {email}");
                    return Redirect("/Auth/Login?error=invalid_credentials");
                }

                // Verify password
                if (!VerifyPassword(password, user.PasswordHash ?? string.Empty))
                {
                    _logger.LogWarning($"Failed login attempt for user: {email}");
                    return Redirect("/Auth/Login?error=invalid_credentials");
                }

                // Generate JWT tokens
                var (accessToken, refreshToken) = await _jwtService.IssueAsync(user);

                // Log successful login
                _logger.LogInformation($"User {email} logged in successfully");

                // Set AccessToken cookie
                Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });

                // Redirect based on role and onboarding status
                var baseUrl = user.OnboardingCompleted ? "/Home" : "/Onboarding/Welcome";

                if (user.Role == "Admin")
                {
                    baseUrl = "/Admin/Dashboard";
                }
                var redirectUrl = $"{baseUrl}?accessToken={Uri.EscapeDataString(accessToken)}&refreshToken={Uri.EscapeDataString(refreshToken)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return Redirect("/Auth/Login?error=server_error");
            }
        }

        /// <summary>
        /// Verify password using PBKDF2
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;

            try
            {
                // Extract salt and hash from stored value (format: salt$hash)
                var parts = hash.Split('$');
                if (parts.Length != 2) return false;

                var saltBytes = Convert.FromBase64String(parts[0]);
                var hashBytes = Convert.FromBase64String(parts[1]);

                // Compute hash of input password
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(32);
                    return computedHash.SequenceEqual(hashBytes);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}