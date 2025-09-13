namespace OrderBook.Application.Services;

/// <inheritdoc />
public class OrderBookState : IOrderBookState
{
    private readonly object _lock = new();
    private IReadOnlyList<(decimal price, decimal amount)> _bids = Array.Empty<(decimal, decimal)>();
    private IReadOnlyList<(decimal price, decimal amount)> _asks = Array.Empty<(decimal, decimal)>();
    private DateTime _ts = DateTime.MinValue;

    /// <inheritdoc />
    public void Update(
        IReadOnlyList<(decimal price, decimal amount)> bids,
        IReadOnlyList<(decimal price, decimal amount)> asks)
    {
        lock (_lock)
        {
            _bids = bids;
            _asks = asks;
            _ts = DateTime.UtcNow;
        }
    }

    /// <inheritdoc />
    public (IReadOnlyList<(decimal price, decimal amount)> bids,
            IReadOnlyList<(decimal price, decimal amount)> asks,
            DateTime timestampUtc) Get()
    {
        lock (_lock)
        {
            return (_bids, _asks, _ts);
        }
    }
}
