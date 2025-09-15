using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OrderBook.Application.Hubs;
using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace OrderBook.Application.Services;

/// <summary>
/// Background service for connecting to Bitstamp WebSocket,
/// updating in-memory state, broadcasting via SignalR,
/// and persisting snapshots into the database.
/// </summary>
public class BitstampService : BackgroundService
{
    private readonly ILogger<BitstampService> _logger;
    private readonly IHubContext<OrderBookHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOrderBookState _state;
    private readonly IBitstampApi _api;
    private readonly IBitstampMessageParser _parser;
    private readonly BitstampOptions _options;

    private readonly Stopwatch _broadcastTimer = Stopwatch.StartNew();
    private readonly Stopwatch _snapshotTimer = Stopwatch.StartNew();
    private const int BroadcastIntervalMs = 500;
    private const int SnapshotMinIntervalMs = 500;
    private string? _lastSnapshotHash = null;

    public BitstampService(
        ILogger<BitstampService> logger,
        IHubContext<OrderBookHub> hubContext,
        IServiceScopeFactory scopeFactory,
        IOrderBookState state,
        IBitstampApi api,
        IBitstampMessageParser parser,
        IOptions<BitstampOptions> options)
    {
        _logger = logger;
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _state = state;
        _api = api;
        _parser = parser;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Step 1: fetch initial snapshot
        var snapshot = await _api.GetSnapshotAsync(stoppingToken);
        _state.Update(snapshot.Bids, snapshot.Asks);
        _logger.LogInformation("Loaded initial snapshot ({b} bids, {a} asks)", snapshot.Bids.Count, snapshot.Asks.Count);

        // Step 2: subscribe to WebSocket
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(_options.WebSocketUrl), stoppingToken);


                var subscribe = new
                {
                    @event = "bts:subscribe",
                    data = new { channel = "order_book_btceur" }
                };

                var msg = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(subscribe));
                await ws.SendAsync(msg, WebSocketMessageType.Text, true, stoppingToken);

                var buffer = new byte[8192];
                while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var sb = new StringBuilder();
                    WebSocketReceiveResult? result;

                    do
                    {
                        result = await ws.ReceiveAsync(buffer, stoppingToken);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    } while (!result.EndOfMessage);

                    if (result?.MessageType == WebSocketMessageType.Close) break;

                    await HandleMessage(sb.ToString(), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bitstamp connection failed, retry in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Processes a WebSocket message using the parser,
    /// updates state, broadcasts to clients, and persists snapshots.
    /// </summary>
    private async Task HandleMessage(string json, CancellationToken token)
    {
        var parsed = _parser.TryParse(json);
        if (parsed is null) return;

        var (bids, asks) = parsed.Value;
        _state.Update(bids, asks);

        if (_broadcastTimer.ElapsedMilliseconds > BroadcastIntervalMs)
        {
            _broadcastTimer.Restart();
            await _hubContext.Clients.All.SendAsync("orderbook:update", new
            {
                bids = bids.Select(b => new[] { b.Price.ToString(), b.Amount.ToString() }),
                asks = asks.Select(a => new[] { a.Price.ToString(), a.Amount.ToString() })
            }, cancellationToken: token);
        }

        var hash = ComputeSnapshotHash(bids, asks);
        if (_snapshotTimer.ElapsedMilliseconds > SnapshotMinIntervalMs && _lastSnapshotHash != hash)
        {
            _snapshotTimer.Restart();
            _lastSnapshotHash = hash;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOrderBookRepository>();
            await repo.SaveSnapshotAsync(bids, asks, token);
        }
    }

    private static string ComputeSnapshotHash(List<OrderLevel> bids, List<OrderLevel> asks, int topN = 100)
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
