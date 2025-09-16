using Microsoft.Extensions.Options;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <summary>
/// Provides access to Bitstamp REST API for fetching order book snapshots.
/// </summary>
public class BitstampApi : IBitstampApi
{
    private readonly HttpClient _http;
    private readonly BitstampOptions _options;

    public BitstampApi(HttpClient http, IOptions<BitstampOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<(List<OrderLevel> Bids, List<OrderLevel> Asks)> GetSnapshotAsync(CancellationToken token)
    {
        var snapshot = await _http.GetFromJsonAsync<BitstampOrderBookResponse>(
            _options.RestUrl,
            cancellationToken: token);

        var bids = snapshot?.Bids
            .Select(b => new OrderLevel(decimal.Parse(b[0]), decimal.Parse(b[1])))
            .ToList() ?? new();

        var asks = snapshot?.Asks
            .Select(a => new OrderLevel(decimal.Parse(a[0]), decimal.Parse(a[1])))
            .ToList() ?? new();

        return (bids, asks);
    }
}

/// <summary>
/// Abstraction for Bitstamp REST API.
/// </summary>
public interface IBitstampApi
{
    /// <summary>
    /// Fetches the current BTC/EUR order book snapshot.
    /// </summary>
    Task<(List<OrderLevel> Bids, List<OrderLevel> Asks)> GetSnapshotAsync(CancellationToken token);
}
