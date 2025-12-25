using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Services;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.Pages.Test;

public class EmailTestModel : PageModel
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailTestModel> _logger;

    public EmailTestModel(IEmailService emailService, ILogger<EmailTestModel> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [BindProperty]
    public EmailTestInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class EmailTestInput
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email người nhận")]
        public string ToEmail { get; set; } = "";

        [Required]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; } = "Test Email từ Money Tracker App";

        [Required]
        [Display(Name = "Nội dung")]
        public string Body { get; set; } = "<h3>Đây là email test</h3><p>Nếu bạn nhận được email này, chức năng gửi email đã hoạt động!</p>";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Attempting to send test email to {Email}", Input.ToEmail);
            
            await _emailService.SendEmailAsync(Input.ToEmail, Input.Subject, Input.Body);
            
            StatusMessage = $"✅ Email đã được gửi thành công tới {Input.ToEmail}! Vui lòng kiểm tra hộp thư.";
            _logger.LogInformation("Test email sent successfully to {Email}", Input.ToEmail);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"❌ Lỗi khi gửi email: {ex.Message}";
            _logger.LogError(ex, "Failed to send test email to {Email}", Input.ToEmail);
        }

        return RedirectToPage();
    }
}
