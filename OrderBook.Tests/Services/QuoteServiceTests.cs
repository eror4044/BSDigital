using OrderBook.Application.Models;
using OrderBook.Application.Services;
using Xunit;

namespace OrderBook.Tests.Services
{
    public class QuoteServiceTests
    {
        [Fact]
        public void GetQuote_WhenSufficientLiquidity_ReturnsCorrectQuote()
        {
            // Arrange
            var state = new OrderBookState();
            state.Update(new List<OrderLevel>(), new List<OrderLevel>
            {
                new(10000m, 0.5m),
                new(11000m, 0.5m)
            });

            var service = new QuoteService(state);

            // Act
            var result = service.GetQuote(0.5m);

            // Assert
            Assert.Equal(0.5m, result.Requested);
            Assert.True(result.Sufficient);
            Assert.Equal(0.5m, result.Filled);
            Assert.Equal(10000m * 0.5m, result.TotalCost);
            Assert.Equal(10000m, result.AveragePrice);
            Assert.True(result.TimestampUtc <= DateTime.UtcNow);
        }

        [Fact]
        public void GetQuote_WhenInsufficientLiquidity_ReturnsPartialFill()
        {
            // Arrange
            var state = new OrderBookState();
            state.Update(new List<OrderLevel>(), new List<OrderLevel>
            {
                new(10000m, 0.3m),
                new(11000m, 0.2m)
            });

            var service = new QuoteService(state);

            // Act
            var result = service.GetQuote(1.0m);

            // Assert
            Assert.Equal(1.0m, result.Requested);
            Assert.False(result.Sufficient);
            Assert.Equal(0.5m, result.Filled);
            Assert.Equal(10000m * 0.3m + 11000m * 0.2m, result.TotalCost);
            Assert.Equal(result.TotalCost / result.Filled, result.AveragePrice);
        }

        [Fact]
        public void GetQuote_WhenNoLiquidity_ReturnsEmptyQuote()
        {
            // Arrange
            var state = new OrderBookState();
            state.Update(new List<OrderLevel>(), new List<OrderLevel>());

            var service = new QuoteService(state);

            // Act
            var result = service.GetQuote(0.5m);

            // Assert
            Assert.Equal(0.5m, result.Requested);
            Assert.False(result.Sufficient);
            Assert.Equal(0, result.Filled);
            Assert.Equal(0, result.TotalCost);
            Assert.Equal(0, result.AveragePrice);
        }
    }
}
