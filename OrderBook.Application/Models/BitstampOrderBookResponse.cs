namespace OrderBook.Application.Models
{
    public record BitstampOrderBookResponse(List<List<string>> Bids, List<List<string>> Asks, string Timestamp);

}
