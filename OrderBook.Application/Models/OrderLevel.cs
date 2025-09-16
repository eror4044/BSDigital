namespace OrderBook.Application.Models
{
    /// <summary>
    /// Represents one level in an order book (price + aggregated amount).
    /// </summary>
    public record OrderLevel(
        decimal Price,
        decimal Amount);
}
