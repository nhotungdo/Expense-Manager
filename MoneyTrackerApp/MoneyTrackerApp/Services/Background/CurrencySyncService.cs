using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Services.Background;

public class CurrencySyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CurrencySyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(1);

    public CurrencySyncService(IServiceProvider serviceProvider, ILogger<CurrencySyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Currency Sync Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
                    await currencyService.SyncRatesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while syncing currency rates.");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("Currency Sync Service is stopping.");
    }
}
