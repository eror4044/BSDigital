using Microsoft.AspNetCore.Mvc;

namespace BSD_fullstack_API.Services;
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
    public IActionResult Get([FromQuery] decimal amount, [FromQuery] string type = "buy")
    {
        if (amount <= 0)
            return BadRequest(new { error = "amount must be > 0" });

        var q = _quotes.GetQuote(amount);
        return Ok(q);
    }
}
