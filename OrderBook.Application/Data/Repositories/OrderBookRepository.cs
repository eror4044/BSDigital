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
            List<OrderLevel> bids,
            List<OrderLevel> asks,
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

        /// <inheritdoc />
        public async Task<List<OrderBookSnapshot>> GetSnapshotsAsync(int take, CancellationToken token)
        {
            return await _context.OrderBookSnapshots
                .OrderByDescending(x => x.Timestamp)
                .Take(take)
                .ToListAsync(token);
        }
    }
}
