namespace BSD_fullstack_API.Services;

public interface IQuoteService
{
    decimal? GetQuote(decimal btcAmount);
}

public class QuoteService : IQuoteService
{
    private readonly OrderBookCache _cache;

    public QuoteService(OrderBookCache cache)
    {
        _cache = cache;
    }

    public decimal? GetQuote(decimal btcAmount)
    {
        if (!_cache.Asks.Any()) return null;

        decimal remaining = btcAmount;
        decimal totalCost = 0;

        foreach (var (price, amount) in _cache.Asks.OrderBy(a => a.Price))
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, amount);
            totalCost += take * price;
            remaining -= take;
        }

        if (remaining > 0) return null;

        return totalCost;
    }
}
