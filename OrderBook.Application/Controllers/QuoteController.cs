using Microsoft.AspNetCore.Mvc;
using OrderBook.Application.Services;

namespace OrderBook.Application.Controllers;

public record QuoteDto(
    decimal RequestedAmountBtc,
    decimal FilledAmountBtc,
    decimal TotalCostEur,
    decimal AveragePriceEur,
    bool SufficientLiquidity,
    DateTime SnapshotTimestampUtc);


[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quotes;
    public QuotesController(IQuoteService quotes) => _quotes = quotes;

    [HttpGet]
    public IActionResult Get([FromQuery] string amount, [FromQuery] string type = "buy")
    {
        if (string.IsNullOrWhiteSpace(amount))
            return BadRequest(new { error = "amount is required" });

        var normalized = amount.Replace(',', '.');

        if (!decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
                              System.Globalization.CultureInfo.InvariantCulture, out var amountVal))
        {
            return BadRequest(new { error = "invalid amount format" });
        }

        if (amountVal <= 0)
            return BadRequest(new { error = "amount must be > 0" });

        var result = _quotes.GetQuote(amountVal);

        return Ok(result);
    }
}
