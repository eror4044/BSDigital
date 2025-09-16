using OrderBook.Application.Models;
namespace OrderBook.Application.Interfaces;

/// <summary>
/// Provides method for calculating quotes based on current order book.
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Calculates a quote for requested BTC amount.
    /// </summary>
    QuoteResult GetQuote(decimal btcAmount);
}