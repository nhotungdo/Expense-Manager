using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IUserManagementService
    {
        Task<List<AdminUserDto>> GetAllUsersAsync(UserFilterDto filter);
        Task<AdminUserDto?> GetUserByIdAsync(long id);
        Task<bool> LockUserAsync(long id, int durationMinutes);
        Task<bool> UnlockUserAsync(long id);
        Task<bool> ResetPasswordAsync(long id);
        Task<bool> AssignRoleAsync(long id, string role);
    }

    public class UserManagementService : IUserManagementService
    {
        private readonly ExpenseManagerContext _context;

        public UserManagementService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync(UserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(u => 
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.Enabled == filter.IsActive.Value);
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return users.Select(u => MapToDto(u)).ToList();
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(long id)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            return user == null ? null : MapToDto(user);
        }

        public async Task<bool> LockUserAsync(long id, int durationMinutes)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(durationMinutes);
            user.Enabled = false; // Also disable login
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.LockoutEnd = null;
            user.Enabled = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString("N");
            
            // Store token
            var existingToken = await _context.AspNetUserTokens
                .FirstOrDefaultAsync(t => t.UserId == id && t.LoginProvider == "Auth" && t.Name == "PasswordReset");
            
            if (existingToken == null)
            {
                _context.AspNetUserTokens.Add(new AspNetUserToken 
                { 
                    UserId = id, 
                    LoginProvider = "Auth", 
                    Name = "PasswordReset",
                    Value = resetToken
                });
            }
            else
            {
                existingToken.Value = resetToken;
            }

            // Queue email
            _context.Emails.Add(new Email 
            { 
                UserId = id, 
                Subject = "Admin Password Reset", 
                Body = $"Your password reset token is: {resetToken}", 
                Status = "Queued",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleAsync(long id, string role)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            user.Role = role; // Update simple string role
            
            await _context.SaveChangesAsync();
            return true;
        }

        private AdminUserDto MapToDto(User user)
        {
            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber,
                IsActive = user.Enabled,
                CreatedAt = user.CreatedAt ?? DateTime.MinValue,
                LastLogin = user.LastLogin,
                Roles = new List<string> { user.Role }, // Using the string role
                IsLocked = user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd
            };
        }
    }
}
