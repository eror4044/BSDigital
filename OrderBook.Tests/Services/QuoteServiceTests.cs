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
            var cache = new OrderBookCache
            {
                Asks = new List<(decimal Price, decimal Amount)>
                {
                    (10000m, 0.5m),
                    (11000m, 0.5m)
                }
            };
            var service = new QuoteService(cache);

            // Act
            var result = service.GetQuote(0.5m);

            // Assert
            Assert.True(result.Sufficient);
            Assert.Equal(0.5m, result.Filled);
            Assert.Equal(10000m * 0.5m, result.TotalCost);
            Assert.Equal(10000m, result.AveragePrice);
        }

        [Fact]
        public void GetQuote_WhenInsufficientLiquidity_ReturnsPartialFill()
        {
            // Arrange
            var cache = new OrderBookCache
            {
                Asks = new List<(decimal Price, decimal Amount)>
                {
                    (10000m, 0.3m),
                    (11000m, 0.2m)
                }
            };
            var service = new QuoteService(cache);

            // Act
            var result = service.GetQuote(1.0m);

            // Assert
            Assert.False(result.Sufficient);
            Assert.Equal(0.5m, result.Filled);
            Assert.Equal(10000m * 0.3m + 11000m * 0.2m, result.TotalCost);
            Assert.Equal(result.TotalCost / result.Filled, result.AveragePrice);
        }

        [Fact]
        public void GetQuote_WhenNoLiquidity_ReturnsEmptyQuote()
        {
            // Arrange
            var cache = new OrderBookCache { Asks = new() };
            var service = new QuoteService(cache);

            // Act
            var result = service.GetQuote(0.5m);

            // Assert
            Assert.False(result.Sufficient);
            Assert.Equal(0, result.Filled);
            Assert.Equal(0, result.TotalCost);
            Assert.Equal(0, result.AveragePrice);
        }
    }
}
