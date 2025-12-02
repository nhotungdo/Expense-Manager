using MoneyTrackerApp.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace MoneyTrackerApp.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly ExpenseManagerContext _db;
        public JwtTokenService(IConfiguration config, ExpenseManagerContext db)
        {
            _config = config;
            _db = db;
        }

        public async Task<(string access, string refresh)> IssueAsync(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? "MoneyTrackerApp";
            var audience = jwtSection.GetValue<string>("Audience") ?? "MoneyTrackerAppClient";
            var key = jwtSection.GetValue<string>("Key") ?? "MoneyTrackerApp-SuperSecret-Key-2025Production";
            var accessMinutes = jwtSection.GetValue<int>("AccessTokenMinutes", 30);
            var refreshDays = jwtSection.GetValue<int>("RefreshTokenDays", 7);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("OnboardingCompleted", user.OnboardingCompleted.ToString())
            };

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var accessToken = new JwtSecurityToken(issuer, audience, claims, now, now.AddMinutes(accessMinutes), creds);
            var access = new JwtSecurityTokenHandler().WriteToken(accessToken);

            var refreshClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("typ", "refresh")
            };
            var refreshToken = new JwtSecurityToken(issuer, audience, refreshClaims, now, now.AddDays(refreshDays), creds);
            var refresh = new JwtSecurityTokenHandler().WriteToken(refreshToken);

            var existingRefresh = _db.AspNetUserTokens.FirstOrDefault(t => t.UserId == user.Id && t.LoginProvider == "Auth" && t.Name == "RefreshToken");
            if (existingRefresh == null)
            {
                existingRefresh = new AspNetUserToken { UserId = user.Id, LoginProvider = "Auth", Name = "RefreshToken" };
                _db.AspNetUserTokens.Add(existingRefresh);
            }
            existingRefresh.Value = refresh;
            await _db.SaveChangesAsync();

            return (access, refresh);
        }
    }
}