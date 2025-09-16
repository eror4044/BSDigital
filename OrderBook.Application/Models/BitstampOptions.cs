namespace OrderBook.Application.Models;

/// <summary>
/// Strongly-typed options for Bitstamp API integration.
/// </summary>
public class BitstampOptions
{
    public string RestUrl { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
}