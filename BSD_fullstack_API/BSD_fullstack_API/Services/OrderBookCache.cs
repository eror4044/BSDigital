namespace BSD_fullstack_API.Services;

public class OrderBookCache
{
    public List<(decimal Price, decimal Amount)> Bids { get; set; } = new();
    public List<(decimal Price, decimal Amount)> Asks { get; set; } = new();
}
