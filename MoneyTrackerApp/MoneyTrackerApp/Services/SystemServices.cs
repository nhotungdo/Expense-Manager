using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MoneyTrackerApp.Hubs;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing notifications
/// Handles notification creation, retrieval, and marking as read
/// </summary>
public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(long userId, bool unreadOnly = false);
    Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto);
    Task<bool> MarkAsReadAsync(long notificationId, long userId);
    Task<bool> MarkAllAsReadAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
    Task SendBudgetAlertNotificationAsync(long userId, BudgetAlertDto alert);
    Task SendDebtReminderNotificationAsync(long userId, DebtResponseDto debt);
    Task SendScheduledTransactionNotificationAsync(long userId, ScheduledTransactionResponseDto scheduled);
    Task DeleteNotificationAsync(long notificationId, long userId);
}

public class NotificationService : INotificationService
{
    private readonly ExpenseManagerContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ExpenseManagerContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(long userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            IsImportant = n.IsImportant,
            CreatedAt = n.CreatedAt ?? DateTime.UtcNow
        }).ToList();
    }

    public async Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            ActionUrl = dto.ActionUrl,
            IsImportant = dto.IsImportant,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var response = new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            IsImportant = notification.IsImportant,
            CreatedAt = notification.CreatedAt ?? DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"User-{dto.UserId}").SendAsync("ReceiveNotification", response);

        return response;
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .FirstOrDefaultAsync();

        if (notification == null)
            return false;

        notification.IsRead = true;
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(long userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task SendBudgetAlertNotificationAsync(long userId, BudgetAlertDto alert)
    {
        await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = $"Budget Alert: {alert.BudgetName}",
            Message = alert.Message,
            Type = "BudgetAlert",
            ActionUrl = $"/budgets/{alert.BudgetId}"
        });
    }

    public async Task SendDebtReminderNotificationAsync(long userId, DebtResponseDto debt)
    {
        var message = debt.DaysRemaining.HasValue && debt.DaysRemaining < 7
            ? $"Debt '{debt.Name}' is due in {debt.DaysRemaining} days. Remaining: {debt.RemainingAmount:N0}"
            : $"You have an outstanding debt: {debt.Name}. Remaining: {debt.RemainingAmount:N0}";

        await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Debt Reminder",
            Message = message,
            Type = "DebtReminder",
            ActionUrl = $"/debts/{debt.Id}"
        });
    }

    public async Task SendScheduledTransactionNotificationAsync(long userId, ScheduledTransactionResponseDto scheduled)
    {
        await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Scheduled Transaction Executed",
            Message = $"Your scheduled transaction '{scheduled.Note}' for {scheduled.Amount:N0} has been executed.",
            Type = "ScheduledTransaction",
            ActionUrl = "/transactions"
        });
    }

    public async Task DeleteNotificationAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}




