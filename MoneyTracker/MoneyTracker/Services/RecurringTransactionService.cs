using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MoneyTracker.Services
{
    public class RecurringTransactionService : BackgroundService
    {
        private readonly ILogger<RecurringTransactionService> _logger;

        public RecurringTransactionService(ILogger<RecurringTransactionService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // TODO: add recurring transaction logic
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RecurringTransactionService error");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}

