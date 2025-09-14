using OrderBook.Application.Services;
using Xunit;

namespace OrderBook.Tests.Services;

public class OrderBookStateTests
{
    [Fact]
    public void Update_ShouldStoreLatestBidsAndAsks()
    {
        // Arrange
        var state = new OrderBookState();
        var bids = new List<(decimal price, decimal amount)> { (10000m, 0.5m) };
        var asks = new List<(decimal price, decimal amount)> { (10100m, 0.3m) };

        // Act
        state.Update(bids, asks);
        var (storedBids, storedAsks, ts) = state.Get();

        // Assert
        Assert.Single(storedBids);
        Assert.Single(storedAsks);
        Assert.Equal(10000m, storedBids[0].price);
        Assert.Equal(10100m, storedAsks[0].price);
        Assert.True(ts <= DateTime.UtcNow);
    }

    [Fact]
    public void Get_ShouldReturnEmptyInitially()
    {
        // Arrange
        var state = new OrderBookState();

        // Act
        var (bids, asks, ts) = state.Get();

        // Assert
        Assert.Empty(bids);
        Assert.Empty(asks);
        Assert.Equal(DateTime.MinValue, ts);
    }

    [Fact]
    public void Update_ShouldBeThreadSafe()
    {
        // Arrange
        var state = new OrderBookState();
        var bids = new List<(decimal price, decimal amount)> { (10000m, 1m) };
        var asks = new List<(decimal price, decimal amount)> { (10100m, 1m) };

        // Act
        Parallel.For(0, 1000, _ => state.Update(bids, asks));

        var (storedBids, storedAsks, ts) = state.Get();

        // Assert
        Assert.NotEmpty(storedBids);
        Assert.NotEmpty(storedAsks);
        Assert.True(ts <= DateTime.UtcNow);
    }
}
