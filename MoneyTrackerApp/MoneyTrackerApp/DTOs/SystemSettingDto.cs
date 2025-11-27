namespace MoneyTrackerApp.DTOs
{
    public class SystemSettingDto
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
        public string Type { get; set; } = null!; // string, int, bool, json
        public bool IsActive { get; set; }
    }

    public class UpdateSystemSettingDto
    {
        public string Value { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
