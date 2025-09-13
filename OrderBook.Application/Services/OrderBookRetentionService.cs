using OrderBook.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace OrderBook.Application.Services;

public class OrderBookRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderBookRetentionService> _logger;
    private const int RetentionDays = 7;

    public OrderBookRetentionService(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderBookRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderBookDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);

                var removed = await db.OrderBookSnapshots
                    .Where(x => x.Timestamp < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (removed > 0)
                    _logger.LogInformation("Retention: deleted {count} snapshots older than {cutoff:u}", removed, cutoff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retention job failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
