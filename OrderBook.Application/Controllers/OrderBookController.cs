using Microsoft.AspNetCore.Mvc;
using OrderBook.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class OrderBookController : ControllerBase
{
    private readonly IOrderBookRepository _repo;

    public OrderBookController(IOrderBookRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("snapshots")]
    public async Task<IActionResult> GetSnapshots([FromQuery] int take = 50, CancellationToken token = default)
    {
        var snapshots = await _repo.GetSnapshotsAsync(take, token);
        return Ok(snapshots);
    }
}
