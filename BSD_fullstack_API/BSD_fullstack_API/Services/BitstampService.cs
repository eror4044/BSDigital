using BSD_fullstack_API.Hubs;
using BSD_fullstack_API.Data;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Diagnostics;

namespace BSD_fullstack_API.Services;

public class BitstampService : BackgroundService
{
    private readonly ILogger<BitstampService> _logger;
    private readonly IHubContext<OrderBookHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OrderBookCache _cache;

    private readonly Stopwatch _broadcastTimer = Stopwatch.StartNew();
    private readonly Stopwatch _snapshotTimer = Stopwatch.StartNew();
    private const int BroadcastIntervalMs = 500;
    private const int SnapshotMinIntervalMs = 500;
    private string? _lastSnapshotHash = null;

    public BitstampService(
        ILogger<BitstampService> logger,
        IHubContext<OrderBookHub> hubContext,
        IServiceScopeFactory scopeFactory,
        OrderBookCache cache)
    {
        _logger = logger;
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                var uri = new Uri("wss://ws.bitstamp.net");
                _logger.LogInformation("Connecting to Bitstamp WebSocket...");
                await ws.ConnectAsync(uri, stoppingToken);

                var subscribe = new
                {
                    @event = "bts:subscribe",
                    data = new { channel = "order_book_btceur" }
                };

                var msg = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(subscribe));
                await ws.SendAsync(msg, WebSocketMessageType.Text, true, stoppingToken);

                _logger.LogInformation("Subscribed request sent");

                var buffer = new byte[8192];

                while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var sb = new StringBuilder();
                    WebSocketReceiveResult? result;

                    do
                    {
                        result = await ws.ReceiveAsync(buffer, stoppingToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("Bitstamp WebSocket closed, reconnecting...");
                            break;
                        }

                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    } while (!result.EndOfMessage);

                    if (result?.MessageType == WebSocketMessageType.Close)
                        break;

                    var json = sb.ToString();

                    try
                    {
                        var obj = JObject.Parse(json);

                        var eventType = (string?)obj["event"];
                        if (eventType == "bts:subscription_succeeded")
                        {
                            _logger.LogInformation("Successfully subscribed to Bitstamp channel");
                            continue;
                        }

                        if (eventType == "data" && obj["data"] != null)
                        {
                            var data = obj["data"];

                            var bids = data["bids"]
                                .Select(b => (Price: decimal.Parse((string)b[0], CultureInfo.InvariantCulture),
                                              Amount: decimal.Parse((string)b[1], CultureInfo.InvariantCulture)))
                                .OrderByDescending(x => x.Price)
                                .ToList();

                            var asks = data["asks"]
                                .Select(a => (Price: decimal.Parse((string)a[0], CultureInfo.InvariantCulture),
                                              Amount: decimal.Parse((string)a[1], CultureInfo.InvariantCulture)))
                                .OrderBy(x => x.Price)
                                .ToList();

                            _cache.Bids = bids;
                            _cache.Asks = asks;

                            if (_broadcastTimer.ElapsedMilliseconds > BroadcastIntervalMs)
                            {
                                _broadcastTimer.Restart();

                                await _hubContext.Clients.All.SendAsync("orderbook:update", new
                                {
                                    bids = bids.Select(b => new[] {
                                        b.Price.ToString(CultureInfo.InvariantCulture),
                                        b.Amount.ToString(CultureInfo.InvariantCulture)
                                    }),
                                    asks = asks.Select(a => new[] {
                                        a.Price.ToString(CultureInfo.InvariantCulture),
                                        a.Amount.ToString(CultureInfo.InvariantCulture)
                                    })
                                }, cancellationToken: stoppingToken);

                                _logger.LogInformation("OrderBook broadcasted ({b} bids, {a} asks)", bids.Count, asks.Count);
                            }

                            var hash = ComputeSnapshotHash(bids, asks);
                            if (_snapshotTimer.ElapsedMilliseconds > SnapshotMinIntervalMs && _lastSnapshotHash != hash)
                            {
                                _snapshotTimer.Restart();
                                _lastSnapshotHash = hash;

                                using var scope = _scopeFactory.CreateScope();
                                var repo = scope.ServiceProvider.GetRequiredService<IOrderBookRepository>();
                                await repo.SaveSnapshotAsync(bids, asks, stoppingToken);

                                _logger.LogInformation("OrderBook snapshot saved (bids={b}, asks={a})", bids.Count, asks.Count);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing message: {json}", json);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bitstamp connection failed, retry in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static string ComputeSnapshotHash(
        List<(decimal Price, decimal Amount)> bids,
        List<(decimal Price, decimal Amount)> asks,
        int topN = 100)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var sb = new StringBuilder();

        foreach (var b in bids.OrderByDescending(x => x.Price).Take(topN))
            sb.Append(b.Price).Append('|').Append(b.Amount).Append(';');

        sb.Append('#');

        foreach (var a in asks.OrderBy(x => x.Price).Take(topN))
            sb.Append(a.Price).Append('|').Append(a.Amount).Append(';');

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
