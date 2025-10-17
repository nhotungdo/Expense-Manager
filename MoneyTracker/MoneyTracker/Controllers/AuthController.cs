using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ExpenseManagerContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(ExpenseManagerContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName, string? UserName);
    public record LoginRequest(string Email, string Password);
    public record GoogleLoginRequest(string GoogleId, string? Email, string? FirstName, string? LastName, string? PictureUrl);

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email and Password are required" });
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var existing = _db.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail);
        if (existing != null)
        {
            return Conflict(new { error = "Email already registered" });
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            UserName = request.UserName ?? request.Email.Trim(),
            NormalizedUserName = (request.UserName ?? request.Email.Trim()).ToUpperInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            FullName = string.Join(' ', new[] { request.FirstName, request.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))),
            GoogleId = string.Empty,
            EmailConfirmed = false,
            Enabled = true,
            CreatedAt = now,
            UpdatedAt = now,
            Role = "User",
            LockoutEnabled = true,
            Language = "vi",
            DefaultCurrency = "VND",
            Timezone = "Asia/Ho_Chi_Minh",
            Theme = "light",
            EmailNotifications = true,
            PushNotifications = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        _db.SaveChanges();

        var token = GenerateJwt(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email and Password are required" });
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = _db.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail);
        if (user == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        user.LastLogin = DateTime.UtcNow;
        _db.SaveChanges();

        var token = GenerateJwt(user);
        return Ok(new { token });
    }

    [HttpPost("google")]
    public IActionResult Google([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GoogleId))
        {
            return BadRequest(new { error = "GoogleId is required" });
        }

        var user = _db.Users.FirstOrDefault(u => u.GoogleId == request.GoogleId);
        if (user == null)
        {
            var now = DateTime.UtcNow;
            user = new User
            {
                GoogleId = request.GoogleId,
                Email = request.Email,
                NormalizedEmail = request.Email?.ToUpperInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = string.Join(' ', new[] { request.FirstName, request.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))),
                ProfilePictureUrl = request.PictureUrl,
                Enabled = true,
                CreatedAt = now,
                UpdatedAt = now,
                Role = "User",
                LockoutEnabled = true,
                Language = "vi",
                DefaultCurrency = "VND",
                Timezone = "Asia/Ho_Chi_Minh",
                Theme = "light",
                EmailNotifications = true,
                PushNotifications = true
            };
            _db.Users.Add(user);
        }
        user.LastLogin = DateTime.UtcNow;
        _db.SaveChanges();

        var token = GenerateJwt(user);
        return Ok(new { token });
    }

    private string GenerateJwt(User user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var key = jwtSection["Key"] ?? string.Empty;
        var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var m) ? m : 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


