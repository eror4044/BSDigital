using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <summary>
/// Represents result of a quote calculation for buying BTC with EUR.
/// </summary>
public record QuoteResult(
    decimal Requested,
    decimal Filled,
    decimal TotalCost,
    decimal AveragePrice,
    bool Sufficient,
    DateTime TimestampUtc);


/// <inheritdoc />
public class QuoteService : IQuoteService
{
    private readonly OrderBookCache _cache;

    public QuoteService(OrderBookCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public QuoteResult GetQuote(decimal btcAmount)
    {
        var asks = _cache.Asks.OrderBy(a => a.Price).ToList();
        if (!asks.Any())
        {
            return new QuoteResult(btcAmount, 0, 0, 0, false, DateTime.UtcNow);
        }

        decimal remaining = btcAmount;
        decimal filled = 0;
        decimal totalCost = 0;

        foreach (var (price, amount) in asks)
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, amount);
            totalCost += take * price;
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