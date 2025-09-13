using Microsoft.AspNetCore.SignalR;

namespace OrderBook.Application.Hubs;

/// <summary>
/// SignalR hub for broadcasting real-time order book updates 
/// (bids, asks, and quotes) to connected clients.
/// </summary>
public class OrderBookHub : Hub
{
    /// <inheritdoc />
    public OrderBookHub() : base()
    {
    }
}
