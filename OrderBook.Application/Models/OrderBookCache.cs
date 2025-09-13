namespace OrderBook.Application.Models;

/// <summary>
/// In-memory cache holding latest bids and asks from Bitstamp stream.
/// </summary>
public class OrderBookCache
{
    /// <summary> Latest list of bids. </summary>
    public List<(decimal Price, decimal Amount)> Bids { get; set; } = new();

    /// <summary> Latest list of asks. </summary>
    public List<(decimal Price, decimal Amount)> Asks { get; set; } = new();
}
