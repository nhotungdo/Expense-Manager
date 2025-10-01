using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ExpenseManagerContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Get user statistics
                var totalTransactions = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .CountAsync() + await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .CountAsync();

                var totalCategories = await _context.Categories.CountAsync();

                var aiSuggestions = await _context.AiSuggestions
                    .Where(a => a.UserId == userId)
                    .CountAsync();

                var accountAge = user.CreatedAt.HasValue ? (DateTime.UtcNow - user.CreatedAt.Value).Days : 0;

                var profileData = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    fullName = user.FullName,
                    phoneNumber = user.PhoneNumber,
                    dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                    gender = user.Gender,
                    address = user.Address,
                    pictureUrl = user.PictureUrl,
                    language = user.Language ?? "vi",
                    defaultCurrency = user.DefaultCurrency ?? "VND",
                    timezone = user.Timezone ?? "Asia/Ho_Chi_Minh",
                    theme = user.Theme ?? "light",
                    emailNotifications = user.EmailNotifications,
                    pushNotifications = user.PushNotifications,
                    totalTransactions,
                    totalCategories,
                    aiSuggestions,
                    accountAge,
                    createdAt = user.CreatedAt,
                    lastLogin = user.LastLogin
                };

                return Ok(profileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile data");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update profile information
                user.FullName = profileDto.FullName;
                user.PhoneNumber = profileDto.PhoneNumber;
                user.DateOfBirth = !string.IsNullOrEmpty(profileDto.DateOfBirth)
                    ? DateOnly.Parse(profileDto.DateOfBirth)
                    : null;
                user.Gender = profileDto.Gender;
                user.Address = profileDto.Address;

                // Update settings
                user.Language = profileDto.Language;
                user.DefaultCurrency = profileDto.DefaultCurrency;
                user.Timezone = profileDto.Timezone;
                user.Theme = profileDto.Theme;
                user.EmailNotifications = profileDto.EmailNotifications;
                user.PushNotifications = profileDto.PushNotifications;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated profile for user {UserId}", userId);

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // In a real application, you would verify the current password
                // For now, we'll just update the password (this is not secure!)
                // TODO: Implement proper password hashing and verification

                if (string.IsNullOrEmpty(passwordDto.NewPassword))
                {
                    return BadRequest("New password is required");
                }

                // For demo purposes, we'll store the password as plain text
                // In production, use proper password hashing like BCrypt
                user.Password = passwordDto.NewPassword; // This should be hashed!

                await _context.SaveChangesAsync();

                _logger.LogInformation("Changed password for user {UserId}", userId);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return BadRequest("Invalid file type. Only JPEG, PNG, and GIF are allowed.");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("File size too large. Maximum size is 5MB.");
                }

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine("wwwroot", "images", "avatars", fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user's picture URL
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.PictureUrl = $"/images/avatars/{fileName}";
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Uploaded avatar for user {UserId}", userId);

                return Ok(new { message = "Avatar uploaded successfully", pictureUrl = user?.PictureUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("login-history")]
        public IActionResult GetLoginHistory([FromQuery] int limit = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // This is a simplified version. In a real application, you would have a proper login history table
                var loginHistory = new List<object>
                {
                    new { timestamp = DateTime.UtcNow.AddHours(-1), ipAddress = "192.168.1.1", userAgent = "Chrome/91.0", location = "Hồ Chí Minh, Việt Nam" },
                    new { timestamp = DateTime.UtcNow.AddHours(-24), ipAddress = "192.168.1.1", userAgent = "Chrome/91.0", location = "Hồ Chí Minh, Việt Nam" },
                    new { timestamp = DateTime.UtcNow.AddDays(-2), ipAddress = "192.168.1.1", userAgent = "Chrome/91.0", location = "Hồ Chí Minh, Việt Nam" },
                    new { timestamp = DateTime.UtcNow.AddDays(-3), ipAddress = "192.168.1.1", userAgent = "Chrome/91.0", location = "Hồ Chí Minh, Việt Nam" },
                    new { timestamp = DateTime.UtcNow.AddDays(-7), ipAddress = "192.168.1.1", userAgent = "Chrome/91.0", location = "Hồ Chí Minh, Việt Nam" }
                };

                return Ok(loginHistory.Take(limit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Verify password (in a real application, you would verify the password)
                if (string.IsNullOrEmpty(deleteDto.Password))
                {
                    return BadRequest("Password is required to delete account");
                }

                // Check if user has transactions
                var hasTransactions = await _context.Expenses.AnyAsync(e => e.UserId == userId) ||
                                    await _context.Incomes.AnyAsync(i => i.UserId == userId);

                if (hasTransactions)
                {
                    return BadRequest("Cannot delete account with existing transactions. Please contact support.");
                }

                // Delete user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted account for user {UserId}", userId);

                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account");
                return StatusCode(500, "Internal server error");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
