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
    private readonly IServiceProvider _serviceProvider; // To resolve TransactionService/AccountService dynamically to avoid circular dependency

    public AutomationService(
        ExpenseManagerContext context, 
        INotificationService notificationService,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _notificationService = notificationService;
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
                if (EvaluateCondition(rule, transaction))
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

    private bool EvaluateCondition(AutomationRule rule, Transaction transaction)
    {
        try
        {
            var condition = JsonSerializer.Deserialize<AutomationConditionDto>(rule.ConditionJson);
            if (condition == null) return false;

            // Only "TransactionCreated" trigger supported for now
            if (rule.TriggerType != "TransactionCreated") return false;

            // Check Transaction Type
            if (condition.TransactionType.HasValue && condition.TransactionType != transaction.TransactionType)
                return false;

            // Check Category
            if (condition.CategoryId.HasValue && condition.CategoryId != transaction.CategoryId)
                return false;

            // Check Amount
            if (condition.AmountThreshold.HasValue)
            {
                if (condition.Operator == ">" && transaction.Amount <= condition.AmountThreshold) return false;
                if (condition.Operator == "<" && transaction.Amount >= condition.AmountThreshold) return false;
                if (condition.Operator == ">=" && transaction.Amount < condition.AmountThreshold) return false;
                if (condition.Operator == "<=" && transaction.Amount > condition.AmountThreshold) return false;
                if (condition.Operator == "==" && transaction.Amount != condition.AmountThreshold) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ExecuteActionAsync(AutomationRule rule, Transaction transaction)
    {
        var action = JsonSerializer.Deserialize<AutomationActionDto>(rule.ActionJson);
        if (action == null) return;

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
                    }
                }
            }
        }
    }
}
