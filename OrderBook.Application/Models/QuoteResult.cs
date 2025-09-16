namespace OrderBook.Application.Models
{
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
}
