using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

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
}

public class NotificationService : INotificationService
{
    private readonly ExpenseManagerContext _context;

    public NotificationService(ExpenseManagerContext context)
    {
        _context = context;
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
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt ?? DateTime.UtcNow
        };
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
}

/// <summary>
/// Service for managing currency rates and conversions
/// </summary>
public interface ICurrencyService
{
    Task<CurrencyRateDto?> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    Task<CurrencyConversionResultDto> ConvertCurrencyAsync(CurrencyConversionDto dto);
    Task UpdateExchangeRatesAsync();
    Task<List<CurrencyRateDto>> GetAllRatesAsync();
}

public class CurrencyService : ICurrencyService
{
    private readonly ExpenseManagerContext _context;

    public CurrencyService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<CurrencyRateDto?> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        var rate = await _context.CurrencyRates
            .Where(cr => cr.FromCurrency == fromCurrency && cr.ToCurrency == toCurrency)
            .FirstOrDefaultAsync();

        if (rate == null)
            return null;

        return new CurrencyRateDto
        {
            FromCurrency = rate.FromCurrency,
            ToCurrency = rate.ToCurrency,
            Rate = rate.Rate,
            LastUpdated = rate.UpdatedAt ?? DateTime.UtcNow
        };
    }

    public async Task<CurrencyConversionResultDto> ConvertCurrencyAsync(CurrencyConversionDto dto)
    {
        if (dto.FromCurrency == dto.ToCurrency)
        {
            return new CurrencyConversionResultDto
            {
                FromCurrency = dto.FromCurrency,
                ToCurrency = dto.ToCurrency,
                OriginalAmount = dto.Amount,
                ConvertedAmount = dto.Amount,
                ExchangeRate = 1,
                RateDate = DateTime.UtcNow
            };
        }

        var rateDto = await GetExchangeRateAsync(dto.FromCurrency, dto.ToCurrency);
        
        if (rateDto == null)
        {
            // Try reverse rate
            var reverseRate = await GetExchangeRateAsync(dto.ToCurrency, dto.FromCurrency);
            if (reverseRate != null)
            {
                var rate = 1 / reverseRate.Rate;
                return new CurrencyConversionResultDto
                {
                    FromCurrency = dto.FromCurrency,
                    ToCurrency = dto.ToCurrency,
                    OriginalAmount = dto.Amount,
                    ConvertedAmount = dto.Amount * rate,
                    ExchangeRate = rate,
                    RateDate = reverseRate.LastUpdated
                };
            }

            throw new InvalidOperationException($"Exchange rate not found for {dto.FromCurrency} to {dto.ToCurrency}");
        }

        return new CurrencyConversionResultDto
        {
            FromCurrency = dto.FromCurrency,
            ToCurrency = dto.ToCurrency,
            OriginalAmount = dto.Amount,
            ConvertedAmount = dto.Amount * rateDto.Rate,
            ExchangeRate = rateDto.Rate,
            RateDate = rateDto.LastUpdated
        };
    }

    public async Task UpdateExchangeRatesAsync()
    {
        // TODO: Integrate with actual currency API (e.g., exchangerate-api.com, fixer.io)
        // For now, update with sample rates
        
        var rates = new Dictionary<(string, string), decimal>
        {
            { ("USD", "VND"), 24000m },
            { ("EUR", "VND"), 26000m },
            { ("GBP", "VND"), 30000m },
            { ("JPY", "VND"), 160m },
            { ("USD", "EUR"), 0.92m },
            { ("USD", "GBP"), 0.79m }
        };

        foreach (var ((from, to), rate) in rates)
        {
            var existing = await _context.CurrencyRates
                .FirstOrDefaultAsync(cr => cr.FromCurrency == from && cr.ToCurrency == to);

            if (existing != null)
            {
                existing.Rate = rate;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.CurrencyRates.Update(existing);
            }
            else
            {
                _context.CurrencyRates.Add(new CurrencyRate
                {
                    FromCurrency = from,
                    ToCurrency = to,
                    Rate = rate,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<CurrencyRateDto>> GetAllRatesAsync()
    {
        var rates = await _context.CurrencyRates.ToListAsync();

        return rates.Select(r => new CurrencyRateDto
        {
            FromCurrency = r.FromCurrency,
            ToCurrency = r.ToCurrency,
            Rate = r.Rate,
            LastUpdated = r.UpdatedAt ?? DateTime.UtcNow
        }).ToList();
    }
}


