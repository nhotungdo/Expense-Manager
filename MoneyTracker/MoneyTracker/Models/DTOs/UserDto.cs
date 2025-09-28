namespace MoneyTracker.Models.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PictureUrl { get; set; }
        public string Role { get; set; } = "USER";
    }

    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
