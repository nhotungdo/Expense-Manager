using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert default admin user
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM users WHERE email = 'nhotungdo89@gmail.com')
                BEGIN
                    INSERT INTO users (
                        google_id, 
                        username, 
                        email, 
                        full_name, 
                        picture_url, 
                        role, 
                        enabled, 
                        created_at, 
                        updated_at, 
                        last_login,
                        language,
                        default_currency,
                        timezone,
                        theme,
                        email_notifications,
                        push_notifications
                    ) VALUES (
                        'admin_default_google_id',
                        'nhotungdo89',
                        'nhotungdo89@gmail.com',
                        'Admin User',
                        '',
                        'ADMIN',
                        1,
                        GETDATE(),
                        GETDATE(),
                        GETDATE(),
                        'vi',
                        'VND',
                        'Asia/Ho_Chi_Minh',
                        'light',
                        1,
                        1
                    )
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove default admin user
            migrationBuilder.Sql(@"
                DELETE FROM users WHERE email = 'nhotungdo89@gmail.com'
            ");
        }
    }
}
