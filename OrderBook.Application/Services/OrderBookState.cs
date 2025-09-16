using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <inheritdoc />
public class OrderBookState : IOrderBookState
{
    private readonly object _lock = new();
    private IReadOnlyList<OrderLevel> _bids = Array.Empty<OrderLevel>();
    private IReadOnlyList<OrderLevel> _asks = Array.Empty<OrderLevel>();
    private DateTime _ts = DateTime.MinValue;

    /// <inheritdoc />
    public void Update(
        IReadOnlyList<OrderLevel> bids,
        IReadOnlyList<OrderLevel> asks)
    {
        lock (_lock)
        {
            _bids = bids;
            _asks = asks;
            _ts = DateTime.UtcNow;
        }
    }

    /// <inheritdoc />
    public (IReadOnlyList<OrderLevel> bids,
            IReadOnlyList<OrderLevel> asks,
            DateTime timestampUtc) Get()
    {
        lock (_lock)
        {
            return (_bids, _asks, _ts);
        }
    }
}
