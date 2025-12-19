namespace MoneyTrackerApp.Configurations
{
    public class EmailSettings
    {
        public string MailServer { get; set; } = null!;
        public int MailPort { get; set; }
        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSsl { get; set; }
    }
}
