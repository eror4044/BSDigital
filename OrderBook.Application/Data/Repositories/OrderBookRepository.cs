using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;
using System.Text.Json;

namespace OrderBook.Application.Data.Repositories
{
    

    /// <inheritdoc />
    public class OrderBookRepository : IOrderBookRepository
    {
        private readonly OrderBookDbContext _context;

        public OrderBookRepository(OrderBookDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task SaveSnapshotAsync(IEnumerable<object> bids, IEnumerable<object> asks, CancellationToken token)
        {
            var snapshot = new OrderBookSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Bids = JsonSerializer.Serialize(bids),
                Asks = JsonSerializer.Serialize(asks)
            };

            _context.OrderBookSnapshots.Add(snapshot);
            await _context.SaveChangesAsync(token);
        }

        /// <inheritdoc />
        public async Task SaveSnapshotAsync(List<(decimal Price, decimal Amount)> bids,
                                            List<(decimal Price, decimal Amount)> asks,
                                            CancellationToken token)
        {
            var snapshot = new OrderBookSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Bids = JsonSerializer.Serialize(bids),
                Asks = JsonSerializer.Serialize(asks)
            };

            _context.OrderBookSnapshots.Add(snapshot);
            await _context.SaveChangesAsync(token);
        }
    }
}
