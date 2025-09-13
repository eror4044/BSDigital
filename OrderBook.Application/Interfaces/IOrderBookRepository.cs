namespace OrderBook.Application.Interfaces
{
    /// <summary>
    /// Repository interface for persisting order book snapshots.
    /// </summary>
    public interface IOrderBookRepository
    {
        /// <summary>
        /// Saves snapshot using generic enumerable objects.
        /// </summary>
        Task SaveSnapshotAsync(IEnumerable<object> bids, IEnumerable<object> asks, CancellationToken token);

        /// <summary>
        /// Saves snapshot using typed bid/ask lists.
        /// </summary>
        Task SaveSnapshotAsync(List<(decimal Price, decimal Amount)> bids,
                               List<(decimal Price, decimal Amount)> asks,
                               CancellationToken stoppingToken);
    }
}
