using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class DefaultAdminService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<DefaultAdminService> _logger;

        public DefaultAdminService(ExpenseManagerContext context, ILogger<DefaultAdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EnsureDefaultAdminExistsAsync()
        {
            try
            {
                var adminEmail = "nhotungdo89@gmail.com";
                var existingAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == adminEmail);

                if (existingAdmin == null)
                {
                    var adminUser = new User
                    {
                        GoogleId = "admin_default_google_id",
                        Username = "nhotungdo89",
                        Email = adminEmail,
                        FullName = "Admin User",
                        PictureUrl = "",
                        Role = "ADMIN",
                        Enabled = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow,
                        Language = "vi",
                        DefaultCurrency = "VND",
                        Timezone = "Asia/Ho_Chi_Minh",
                        Theme = "light",
                        EmailNotifications = true,
                        PushNotifications = true
                    };

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default admin user created: {Email}", adminEmail);
                }
                else if (existingAdmin.Role != "ADMIN")
                {
                    // Update existing user to admin role
                    existingAdmin.Role = "ADMIN";
                    existingAdmin.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Existing user promoted to admin: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogInformation("Default admin user already exists: {Email}", adminEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring default admin user exists");
            }
        }
    }
}
