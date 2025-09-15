using Microsoft.EntityFrameworkCore;
using OrderBook.Application.Data;
using OrderBook.Application.Data.Repositories;
using OrderBook.Application.Models;

namespace OrderBook.Tests.Data;

public class OrderBookRepositoryTests
{
    private OrderBookDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderBookDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderBookDbContext(options);
    }

    [Fact]
    public async Task SaveSnapshotAsync_ShouldInsertSnapshot()
    {
        using var context = GetInMemoryDbContext();
        var repo = new OrderBookRepository(context);

        var bids = new List<OrderLevel>
        {
            new(10000m, 0.5m),
            new(10100m, 0.3m)
        };

        var asks = new List<OrderLevel>
        {
            new(10200m, 0.4m),
            new(10300m, 0.6m)
        };

        await repo.SaveSnapshotAsync(bids, asks, CancellationToken.None);

        var snapshot = context.OrderBookSnapshots.SingleOrDefault();
        Assert.NotNull(snapshot);
        Assert.NotEmpty(snapshot.Bids);
        Assert.NotEmpty(snapshot.Asks);
        Assert.True(snapshot.Timestamp <= DateTime.UtcNow);
    }
}
