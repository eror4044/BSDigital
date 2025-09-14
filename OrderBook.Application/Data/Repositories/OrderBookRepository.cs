using Microsoft.EntityFrameworkCore;
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
        public async Task SaveSnapshotAsync(
            List<(decimal Price, decimal Amount)> bids,
            List<(decimal Price, decimal Amount)> asks,
            CancellationToken token)
        {
            var bidLevels = bids.Select(b => new OrderLevel(b.Price, b.Amount)).ToList();
            var askLevels = asks.Select(a => new OrderLevel(a.Price, a.Amount)).ToList();

            var snapshot = new OrderBookSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Bids = JsonSerializer.Serialize(bidLevels),
                Asks = JsonSerializer.Serialize(askLevels)
            };

            _context.OrderBookSnapshots.Add(snapshot);
            await _context.SaveChangesAsync(token);
        }

        /// <inheritdoc />
        public async Task<List<OrderBookSnapshot>> GetSnapshotsAsync(int take, CancellationToken token)
        {
            return await _context.OrderBookSnapshots
                .OrderByDescending(x => x.Timestamp)
                .Take(take)
                .ToListAsync(token);
        }
    }

    public record OrderLevel(decimal Price, decimal Amount);
}
