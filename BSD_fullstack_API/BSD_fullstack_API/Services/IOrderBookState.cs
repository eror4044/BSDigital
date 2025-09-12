namespace BSD_fullstack_API.Services;

public interface IOrderBookState
{
    void Update(
        IReadOnlyList<(decimal price, decimal amount)> bids,
        IReadOnlyList<(decimal price, decimal amount)> asks);

    (IReadOnlyList<(decimal price, decimal amount)> bids,
     IReadOnlyList<(decimal price, decimal amount)> asks,
     DateTime timestampUtc) Get();
}
