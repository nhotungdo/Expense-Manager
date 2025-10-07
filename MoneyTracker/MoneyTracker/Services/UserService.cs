using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class UserService : IUserService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IAuditService _auditService;

        public UserService(ExpenseManagerContext context, ILogger<UserService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User> CreateUserAsync(UserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FullName = userDto.FullName,
                GoogleId = userDto.GoogleId,
                PictureUrl = userDto.PictureUrl,
                DefaultCurrency = userDto.DefaultCurrency ?? "VND",
                Language = userDto.Language ?? "vi",
                Theme = userDto.Theme ?? "light",
                Timezone = userDto.Timezone ?? "Asia/Ho_Chi_Minh",
                Role = userDto.Role ?? "USER",
                Enabled = true,
                EmailNotifications = true,
                PushNotifications = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(user.Id, "CREATE", $"User account created: {user.Email}", "User", user.Id);

            _logger.LogInformation("User created: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }

        public async Task<User?> UpdateUserAsync(long id, UserDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            var oldEmail = user.Email;
            var oldFullName = user.FullName;

            user.Username = userDto.Username ?? user.Username;
            user.Email = userDto.Email ?? user.Email;
            user.FullName = userDto.FullName ?? user.FullName;
            user.PictureUrl = userDto.PictureUrl ?? user.PictureUrl;
            user.DefaultCurrency = userDto.DefaultCurrency ?? user.DefaultCurrency;
            user.Language = userDto.Language ?? user.Language;
            user.Theme = userDto.Theme ?? user.Theme;
            user.Timezone = userDto.Timezone ?? user.Timezone;
            user.Role = userDto.Role ?? user.Role;
            user.PhoneNumber = userDto.PhoneNumber ?? user.PhoneNumber;
            user.Address = userDto.Address ?? user.Address;
            user.DateOfBirth = userDto.DateOfBirth ?? user.DateOfBirth;
            user.Gender = userDto.Gender ?? user.Gender;
            user.EmailNotifications = userDto.EmailNotifications;
            user.PushNotifications = userDto.PushNotifications;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(id, "UPDATE",
                $"User profile updated: {oldFullName} ({oldEmail})", "User", id);

            _logger.LogInformation("User updated: {UserId}", id);
            return user;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            // Soft delete - disable user instead of hard delete
            user.Enabled = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(id, "DELETE", $"User account disabled: {user.Email}", "User", id);

            _logger.LogInformation("User disabled: {UserId}", id);
            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "LOGIN", "User logged in", "User", userId);

            _logger.LogInformation("User login updated: {UserId}", userId);
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(int skip = 0, int take = 50)
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Enabled = !user.Enabled;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var action = user.Enabled ? "ENABLED" : "DISABLED";
            await _auditService.LogUserActionAsync(id, action, $"User account {action.ToLower()}: {user.Email}", "User", id);

            _logger.LogInformation("User status toggled: {UserId} - {Status}", id, user.Enabled ? "Enabled" : "Disabled");
            return true;
        }

        public async Task<Dictionary<string, object>> GetUserStatsAsync(long userId)
        {
            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.Amount);

            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.Amount);

            var expenseCount = await _context.Expenses
                .Where(e => e.UserId == userId)
                .CountAsync();

            var incomeCount = await _context.Incomes
                .Where(i => i.UserId == userId)
                .CountAsync();

            var categoryCount = await _context.Categories
                .Where(c => c.UserId == userId)
                .CountAsync();

            var lastExpense = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            var lastIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            return new Dictionary<string, object>
            {
                ["TotalExpenses"] = totalExpenses,
                ["TotalIncome"] = totalIncome,
                ["NetWorth"] = totalIncome - totalExpenses,
                ["ExpenseCount"] = expenseCount,
                ["IncomeCount"] = incomeCount,
                ["CategoryCount"] = categoryCount,
                ["LastExpenseDate"] = lastExpense?.CreatedAt,
                ["LastIncomeDate"] = lastIncome?.CreatedAt
            };
        }
    }
}
