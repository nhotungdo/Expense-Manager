using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.Pages.Admin.Notifications;

[Authorize(Roles = "Admin")]
public class EmailSenderModel : PageModel
{
    private readonly IEmailService _emailService;
    private readonly ExpenseManagerContext _context;

    public EmailSenderModel(IEmailService emailService, ExpenseManagerContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    [BindProperty]
    public EmailInputModel Input { get; set; } = new();

    public List<Email> EmailLogs { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = "";

    public class EmailInputModel
    {
        [Required]
        [Display(Name = "To (Separated by comma)")]
        public string To { get; set; } = "";

        [Required]
        public string Subject { get; set; } = "";

        [Required]
        public string Body { get; set; } = "";

        [Display(Name = "Schedule (Optional)")]
        public DateTime? ScheduleTime { get; set; }

        [Display(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadLogsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadLogsAsync();
            return Page();
        }

        var recipients = Input.To.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(e => e.Trim())
                                 .ToList();

        if (Input.ScheduleTime.HasValue && Input.ScheduleTime.Value > DateTime.UtcNow)
        {
            // Scheduling logic
            foreach (var recipient in recipients)
            {
                var email = new Email
                {
                    RecipientEmail = recipient,
                    Subject = Input.Subject,
                    Body = Input.Body,
                    ScheduledAt = Input.ScheduleTime.Value,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                };
                
                // Link user if exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == recipient);
                if (user != null) email.UserId = user.Id;

                _context.Emails.Add(email);
            }
            await _context.SaveChangesAsync();
            StatusMessage = "Emails scheduled successfully.";
        }
        else
        {
            // Send immediately
            await _emailService.SendEmailAsync(recipients, Input.Subject, Input.Body, Input.Attachments);
            StatusMessage = "Emails sent successfully.";
        }

        return RedirectToPage();
    }

    private async Task LoadLogsAsync()
    {
        EmailLogs = await _context.Emails
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
}
