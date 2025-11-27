using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs
{
    public class AdminUserDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class LockUserDto
    {
        public int DurationMinutes { get; set; }
    }

    public class AssignRoleDto
    {
        public string Role { get; set; } = null!;
    }
}
