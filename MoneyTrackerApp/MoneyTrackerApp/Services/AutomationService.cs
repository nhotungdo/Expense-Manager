using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services;

public interface IAutomationService
{
    Task CheckAndExecuteRulesAsync(Transaction transaction);
    Task ExecuteScheduledAutomationsAsync(); // For future cron jobs
}

public class AutomationService : IAutomationService
{
    private readonly ExpenseManagerContext _context;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IServiceProvider _serviceProvider; // To resolve TransactionService/AccountService dynamically to avoid circular dependency

    public AutomationService(
        ExpenseManagerContext context, 
        INotificationService notificationService,
        IEmailService emailService,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
        _serviceProvider = serviceProvider;
    }

    public async Task CheckAndExecuteRulesAsync(Transaction transaction)
    {
        // 1. Get active rules for this user
        var rules = await _context.AutomationRules
            .Where(r => r.UserId == transaction.UserId && r.IsActive)
            .ToListAsync();

        foreach (var rule in rules)
        {
            try
            {
                if (await EvaluateConditionAsync(rule, transaction))
                {
                    await ExecuteActionAsync(rule, transaction);
                    
                    // Update stats
                    rule.LastExecutedAt = DateTime.UtcNow;
                    _context.AutomationRules.Update(rule);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop transaction
                Console.WriteLine($"Error executing rule {rule.Id}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task ExecuteScheduledAutomationsAsync()
    {
        // Placeholder for time-based rules
        await Task.CompletedTask;
    }

    private async Task<bool> EvaluateConditionAsync(AutomationRule rule, Transaction transaction)
    {
        try
        {
            var condition = JsonSerializer.Deserialize<AutomationConditionDto>(rule.ConditionJson);
            if (condition == null) return false;

            // Only "TransactionCreated" trigger supported for now
            if (rule.TriggerType != "TransactionCreated") return false;

            // Common Check: Account Source
            if (condition.AccountId.HasValue && condition.AccountId != transaction.AccountId)
                return false;

            if (condition.CheckType == "SpendingLimit")
            {
                // Calculate total spending in period
                if (!condition.AmountThreshold.HasValue) return false;
                if (!condition.CategoryId.HasValue) return false; // Must specify category for spending limit usually

                // Limit query to this user
                var query = _context.Transactions
                    .Where(t => t.UserId == transaction.UserId 
                             && t.TransactionType == 2); // Expense

                if (condition.CategoryId.HasValue)
                    query = query.Where(t => t.CategoryId == condition.CategoryId.Value);
                if (condition.AccountId.HasValue)
                    query = query.Where(t => t.AccountId == condition.AccountId.Value);

                // Date Range
                DateTime startDate = DateTime.UtcNow;
                if (condition.Period == "Weekly")
                    startDate = DateTime.UtcNow.AddDays(-7);
                else // Monthly default
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                query = query.Where(t => t.TransactionDate >= startDate);

                var totalSpending = await query.SumAsync(t => t.Amount);

                // Check Threshold
                // "Exceeds" -> Total > Threshold
                return totalSpending > condition.AmountThreshold.Value;
            }
            else if (condition.CheckType == "Balance")
            {
                if (!condition.AmountThreshold.HasValue) return false;
                
                // Reload Balance to be sure
                var account = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == transaction.AccountId);
                
                if (account == null) return false;

                return CompareAmount(account.CurrentBalance, condition.AmountThreshold.Value, condition.Operator ?? "<");
            }
            else // Default: Transaction Properties Check
            {
                // Check Transaction Type
                if (condition.TransactionType.HasValue && condition.TransactionType != transaction.TransactionType)
                    return false;

                // Check Category
                if (condition.CategoryId.HasValue && condition.CategoryId != transaction.CategoryId)
                    return false;

                // Check Amount
                if (condition.AmountThreshold.HasValue)
                {
                    return CompareAmount(transaction.Amount, condition.AmountThreshold.Value, condition.Operator);
                }

                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool CompareAmount(decimal value, decimal threshold, string? op)
    {
        if (op == ">") return value > threshold;
        if (op == "<") return value < threshold;
        if (op == ">=") return value >= threshold;
        if (op == "<=") return value <= threshold;
        if (op == "==") return value == threshold;
        return false;
    }

    private async Task ExecuteActionAsync(AutomationRule rule, Transaction transaction)
    {
        var action = JsonSerializer.Deserialize<AutomationActionDto>(rule.ActionJson);
        if (action == null) return;

        bool success = false;

        if (action.Type == "Notify")
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = rule.UserId,
                Title = $"Automation: {rule.Name}",
                Message = action.Message ?? "Quy tắc tự động đã được kích hoạt.",
                Type = "Automation",
                ActionUrl = "/Transactions"
            });
            success = true;
        }
        else if (action.Type == "Transfer")
        {
            // Auto-saving logic: Transfer % or fixed amount to another wallet
            if (action.TargetAccountId.HasValue && action.Amount > 0)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                    
                    decimal transferAmount = action.Amount;
                    if (action.IsPercentage)
                    {
                        transferAmount = transaction.Amount * (action.Amount / 100);
                    }

                    try 
                    {
                        await transactionService.CreateTransactionAsync(rule.UserId, new CreateTransactionDto
                        {
                            AccountId = transaction.AccountId,
                            PairedAccountId = action.TargetAccountId.Value,
                            Amount = transferAmount,
                            TransactionType = 3, // Transfer
                            TransactionDate = DateTime.UtcNow,
                            Note = $"Auto-transfer: {rule.Name}"
                        });
                        success = true;
                    }
                    catch (Exception ex)
                    {
                         await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = rule.UserId,
                            Title = $"Automation Failed: {rule.Name}",
                            Message = $"Could not execute transfer: {ex.Message}",
                            Type = "System",
                            IsImportant = true
                        });
                        success = false;
                    }
                }
            }
        }

        if (success)
        {
            // 1. Send Email Notification
            var user = await _context.Users.FindAsync(rule.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                try 
                {
                    await _emailService.SendEmailAsync(user.Email, 
                        $"[MoneyTracker] Quy tắc tự động '{rule.Name}' đã được thực thi", 
                        $"<p>Xin chào {user.UserName},</p><p>Quy tắc tự động <strong>{rule.Name}</strong> của bạn vừa được thực thi thành công.</p>");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send automation email: {ex.Message}");
                }
            }

            // 2. Display Notification (Only if the action wasn't already a Notify, to avoid Double Notification)
            // But user said: "Send email AND Show Notification". 
            // If action was Notify, we showed a notification (custom message). That counts.
            // If action wasn't Notify, we haven't shown a success notification yet. So show it.
            if (action.Type != "Notify")
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = rule.UserId,
                    Title = $"Automation Executed: {rule.Name}",
                    Message = "Quy tắc tự động đã chạy thành công.",
                    Type = "Automation",
                    ActionUrl = "/Automation"
                });
            }
        }
    }
}
