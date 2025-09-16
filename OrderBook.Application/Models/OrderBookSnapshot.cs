namespace OrderBook.Application.Models;

/// <summary>
/// Entity representing persisted order book snapshot in PostgreSQL.
/// </summary>
public class OrderBookSnapshot
{
    /// <summary> Primary key. </summary>
    public int Id { get; set; }

    /// <summary> UTC timestamp when snapshot was taken. </summary>
    public DateTime Timestamp { get; set; }

    /// <summary> Serialized bids in JSON format. </summary>
    public string Bids { get; set; } = string.Empty;

    /// <summary> Serialized asks in JSON format. </summary>
    public string Asks { get; set; } = string.Empty;
}
