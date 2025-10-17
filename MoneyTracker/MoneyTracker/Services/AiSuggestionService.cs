using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class AiSuggestionService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AiSuggestionService> _logger;

    public AiSuggestionService(IServiceProvider services, ILogger<AiSuggestionService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();

                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var prevMonthStart = monthStart.AddMonths(-1);
                var prevMonthEnd = monthStart.AddDays(-1);

                var users = await db.Users.Select(u => new { u.Id }).ToListAsync(stoppingToken);
                foreach (var u in users)
                {
                    var currentByCat = await db.Transactions
                        .Where(t => t.UserId == u.Id && t.Type == 0 && t.TransactionDate >= monthStart)
                        .GroupBy(t => t.CategoryId)
                        .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
                        .ToListAsync(stoppingToken);

                    var prevByCat = await db.Transactions
                        .Where(t => t.UserId == u.Id && t.Type == 0 && t.TransactionDate >= prevMonthStart && t.TransactionDate <= prevMonthEnd)
                        .GroupBy(t => t.CategoryId)
                        .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount) })
                        .ToListAsync(stoppingToken);

                    foreach (var c in currentByCat)
                    {
                        var prev = prevByCat.FirstOrDefault(x => x.CategoryId == c.CategoryId)?.Total ?? 0m;
                        if (prev == 0 && c.Total == 0) continue;
                        var change = prev == 0 ? 100 : (double)((c.Total - prev) / prev) * 100.0;
                        if (change >= 30)
                        {
                            var suggestion = new AiSuggestion
                            {
                                UserId = u.Id,
                                Suggestion = $"This month, you spent {change:F0}% more on category {c.CategoryId} compared to last month.",
                                SuggestionType = "Spending Spike",
                                CreatedAt = DateTime.UtcNow
                            };
                            db.AiSuggestions.Add(suggestion);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiSuggestionService loop error");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}


