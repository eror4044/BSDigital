namespace OrderBook.Application.Services;

/// <summary>
/// Provides abstraction for maintaining the current in-memory state of the order book.
/// </summary>
public interface IOrderBookState
{

    /// <summary>
    /// Updates order book with latest bids and asks.
    /// </summary>
    void Update(
        IReadOnlyList<(decimal price, decimal amount)> bids,
        IReadOnlyList<(decimal price, decimal amount)> asks);

    /// <summary>
    /// Returns current bids, asks and the timestamp of last update.
    /// </summary>
    (IReadOnlyList<(decimal price, decimal amount)> bids,
     IReadOnlyList<(decimal price, decimal amount)> asks,
     DateTime timestampUtc) Get();
}
