using OrderBook.Application.Models;

namespace OrderBook.Application.Interfaces
{
    /// <summary>
    /// Repository interface for persisting order book snapshots.
    /// </summary>
    public interface IOrderBookRepository
    {
        /// <summary>
        /// Saves snapshot using typed bid/ask lists.
        /// </summary>
        Task SaveSnapshotAsync(
            List<OrderLevel> bids,
            List<OrderLevel> asks,
            CancellationToken token);

        /// <summary>
        /// Returns the latest persisted snapshots of the order book,
        /// ordered from newest to oldest.
        /// </summary>
        /// <param name="take">Maximum number of snapshots to return.</param>
        /// <param name="token">Cancellation token.</param>
        Task<List<OrderBookSnapshot>> GetSnapshotsAsync(
            int take,
            CancellationToken token);
    }
}
