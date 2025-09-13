namespace OrderBook.Application.Models
{
    public class OrderBookSnapshot
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public string Bids { get; set; } = string.Empty;
        public string Asks { get; set; } = string.Empty;
    }
}
