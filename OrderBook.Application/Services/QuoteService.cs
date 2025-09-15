using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <inheritdoc />
public class QuoteService : IQuoteService
{
    private readonly IOrderBookState _state;

    public QuoteService(IOrderBookState state)
    {
        _state = state;
    }

    /// <inheritdoc />
    public QuoteResult GetQuote(decimal btcAmount)
    {
        var (bids, asks, _) = _state.Get();
        var orderedAsks = asks.OrderBy(a => a.Price).ToList();

        if (!orderedAsks.Any())
        {
            return new QuoteResult(btcAmount, 0, 0, 0, false, DateTime.UtcNow);
        }

        decimal remaining = btcAmount;
        decimal filled = 0;
        decimal totalCost = 0;

        foreach (var level in orderedAsks)
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, level.Amount);
            totalCost += take * level.Price;
            filled += take;
            remaining -= take;
        }

        bool sufficient = (filled >= btcAmount);
        decimal avg = filled > 0 ? totalCost / filled : 0;

        return new QuoteResult(
            Requested: btcAmount,
            Filled: filled,
            TotalCost: totalCost,
            AveragePrice: avg,
            Sufficient: sufficient,
            TimestampUtc: DateTime.UtcNow
        );
    }
}
