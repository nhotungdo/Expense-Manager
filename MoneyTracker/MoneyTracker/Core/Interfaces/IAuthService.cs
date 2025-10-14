using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(User user);
    Task<User?> AuthenticateGoogleUserAsync(string googleToken);
    Task<User> CreateUserFromGoogleAsync(string googleId, string email, string fullName, string? pictureUrl);
    Task<bool> ValidateTokenAsync(string token);
    Task<User?> GetUserFromTokenAsync(string token);
}
