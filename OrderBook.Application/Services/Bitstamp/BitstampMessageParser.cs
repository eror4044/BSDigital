using System.Globalization;
using System.Text.Json;
using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <summary>
/// Parses Bitstamp WebSocket messages into typed order book updates.
/// </summary>
public class BitstampMessageParser : IBitstampMessageParser
{
    /// <inheritdoc />
    public (List<OrderLevel> Bids, List<OrderLevel> Asks)? TryParse(string json)
    {
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("event", out var eventProp))
        {
            var eventType = eventProp.GetString();
            if (eventType == "data" && doc.RootElement.TryGetProperty("data", out var data))
            {
                var bids = data.GetProperty("bids")
                    .EnumerateArray()
                    .Select(b => new OrderLevel(ParseDecimal(b[0].GetString()!), ParseDecimal(b[1].GetString()!)))
                    .OrderByDescending(x => x.Price)
                    .ToList();

                var asks = data.GetProperty("asks")
                    .EnumerateArray()
                    .Select(a => new OrderLevel(ParseDecimal(a[0].GetString()!), ParseDecimal(a[1].GetString()!)))
                    .OrderBy(x => x.Price)
                    .ToList();

                return (bids, asks);
            }
        }

        return null;
    }

    private static decimal ParseDecimal(string value) =>
        decimal.Parse(value, CultureInfo.InvariantCulture);
}
