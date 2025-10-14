using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var issuer = jwtSettings["Issuer"] ?? "MoneyTracker";
        var audience = jwtSettings["Audience"] ?? "MoneyTrackerUsers";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.FullName ?? ""),
            new(ClaimTypes.Role, user.Role),
            new("GoogleId", user.GoogleId),
            new("ProfilePictureUrl", user.ProfilePictureUrl ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<User?> AuthenticateGoogleUserAsync(string googleToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

            if (payload == null)
            {
                _logger.LogWarning("Invalid Google token");
                return null;
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

            if (user == null)
            {
                user = await CreateUserFromGoogleAsync(payload.Subject, payload.Email, payload.Name, payload.Picture);
            }
            else
            {
                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating Google user");
            return null;
        }
    }

    public async Task<User> CreateUserFromGoogleAsync(string googleId, string email, string fullName, string? pictureUrl)
    {
        var user = new User
        {
            GoogleId = googleId,
            Email = email,
            UserName = email,
            FirstName = fullName?.Split(' ').FirstOrDefault(),
            LastName = fullName?.Split(' ').Skip(1).FirstOrDefault(),
            FullName = fullName,
            ProfilePictureUrl = pictureUrl,
            Role = "User",
            Enabled = true,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created new user from Google: {Email}", email);
        return user;
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var issuer = jwtSettings["Issuer"] ?? "MoneyTracker";
            var audience = jwtSettings["Audience"] ?? "MoneyTrackerUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Task.FromResult(false);
        }
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return await _unitOfWork.Users.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting user from token");
            return null;
        }
    }
}
