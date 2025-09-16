using OrderBook.Application.Models;

namespace OrderBook.Application.Interfaces;

/// <summary>
/// Provides abstraction for maintaining the current in-memory state of the order book.
/// </summary>
public interface IOrderBookState
{
    /// <summary>
    /// Updates order book with latest bids and asks.
    /// </summary>
    void Update(
        IReadOnlyList<OrderLevel> bids,
        IReadOnlyList<OrderLevel> asks);

    /// <summary>
    /// Returns current bids, asks and the timestamp of last update.
    /// </summary>
    (IReadOnlyList<OrderLevel> bids,
     IReadOnlyList<OrderLevel> asks,
     DateTime timestampUtc) Get();
}
