using OrderBook.Application.Models;

namespace OrderBook.Application.Interfaces
{
    /// <summary>
    /// Abstraction for parsing Bitstamp WebSocket messages.
    /// </summary>
    public interface IBitstampMessageParser
    {
        /// <summary>
        /// Tries to parse a Bitstamp WebSocket message into order book levels.
        /// </summary>
        (List<OrderLevel> Bids, List<OrderLevel> Asks)? TryParse(string json);
    }
}
