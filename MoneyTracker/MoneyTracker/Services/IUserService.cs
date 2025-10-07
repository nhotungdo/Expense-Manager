using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(long id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByGoogleIdAsync(string googleId);
        Task<User> CreateUserAsync(UserDto userDto);
        Task<User?> UpdateUserAsync(long id, UserDto userDto);
        Task<bool> DeleteUserAsync(long id);
        Task<bool> UpdateLastLoginAsync(long userId);
        Task<IEnumerable<User>> GetUsersAsync(int skip = 0, int take = 50);
        Task<bool> ToggleUserStatusAsync(long id);
        Task<Dictionary<string, object>> GetUserStatsAsync(long userId);
    }
}
