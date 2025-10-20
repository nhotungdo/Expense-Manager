using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Reminders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public IndexModel(ExpenseManagerContext db) { _db = db; }

    public List<Debt> DueDebts { get; set; } = new();
    public List<ScheduledTransaction> TodayScheduled { get; set; } = new();
    public List<Email> QueuedEmails { get; set; } = new();
    public bool Generated { get; set; }

    public async Task OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var today = DateTime.Today;
        var todayDateOnly = DateOnly.FromDateTime(today);

        // Due debts (within 7 days)
        DueDebts = await _db.Debts
            .Where(d => d.UserId == userId && d.Status == 1 && d.DueDate.HasValue && d.DueDate.Value <= DateOnly.FromDateTime(today.AddDays(7)))
            .OrderBy(d => d.DueDate)
            .ToListAsync();

        // Today's scheduled transactions
        TodayScheduled = await _db.ScheduledTransactions
            .Include(s => s.Account)
            .Where(s => s.UserId == userId && s.IsActive && s.NextRunDate == todayDateOnly)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out var userId);
        var today = DateTime.Today;
        var todayDateOnly = DateOnly.FromDateTime(today);
        var emails = new List<Email>();

        // Generate debt reminders
        var dueDebts = await _db.Debts
            .Where(d => d.UserId == userId && d.Status == 1 && d.DueDate.HasValue && d.DueDate.Value <= DateOnly.FromDateTime(today.AddDays(7)))
            .ToListAsync();

        foreach (var debt in dueDebts)
        {
            var daysUntilDue = debt.DueDate!.Value.DayNumber - todayDateOnly.DayNumber;
            var subject = daysUntilDue <= 0 ? $"OVERDUE: {debt.Name}" : $"Reminder: {debt.Name} due in {daysUntilDue} days";
            var body = $"Debt: {debt.Name}\nAmount: {debt.InitialAmount:N2}\nDue: {debt.DueDate:yyyy-MM-dd}\nPerson: {debt.PersonName}";

            emails.Add(new Email
            {
                UserId = userId,
                Subject = subject,
                Body = body,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Generate scheduled transaction reminders
        var todayScheduled = await _db.ScheduledTransactions
            .Include(s => s.Account)
            .Where(s => s.UserId == userId && s.IsActive && s.NextRunDate == DateOnly.FromDateTime(today))
            .ToListAsync();

        foreach (var sched in todayScheduled)
        {
            var subject = $"Scheduled Transaction: {sched.Account?.Name}";
            var body = $"Account: {sched.Account?.Name}\nAmount: {sched.Amount:N2}\nType: {(sched.TransactionType == 0 ? "Expense" : "Income")}\nFrequency: {sched.Frequency}";

            emails.Add(new Email
            {
                UserId = userId,
                Subject = subject,
                Body = body,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (emails.Any())
        {
            _db.Emails.AddRange(emails);
            await _db.SaveChangesAsync();
        }

        QueuedEmails = emails;
        Generated = true;
        await OnGetAsync(); // Refresh data
        return Page();
    }
}
