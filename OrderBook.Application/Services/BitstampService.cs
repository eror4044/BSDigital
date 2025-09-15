using System.Diagnostics;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrderBook.Application.Data.Repositories;
using OrderBook.Application.Hubs;
using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;

namespace OrderBook.Application.Services;

/// <summary>
/// Background service for connecting to Bitstamp API/WebSocket,
/// maintaining BTC/EUR order book state,
/// broadcasting updates via SignalR,
/// and persisting snapshots into the database.
/// </summary>
public class BitstampService : BackgroundService
{
    private readonly ILogger<BitstampService> _logger;
    private readonly IHubContext<OrderBookHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOrderBookState _state;
    private readonly HttpClient _httpClient;

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
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _state = state;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Step 1: fetch initial snapshot via REST
        await LoadInitialSnapshot(stoppingToken);

        // Step 2: start WebSocket listener
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

                var msg = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(subscribe));
                await ws.SendAsync(msg, WebSocketMessageType.Text, true, stoppingToken);

                _logger.LogInformation("Subscribed to order_book_btceur");

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
                            _logger.LogWarning("WebSocket closed, reconnecting...");
                            break;
                        }

                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    } while (!result.EndOfMessage);

                    if (result?.MessageType == WebSocketMessageType.Close)
                        break;

                    await HandleMessage(sb.ToString(), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bitstamp connection failed, retrying in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Fetches initial order book snapshot from REST API.
    /// </summary>
    private async Task LoadInitialSnapshot(CancellationToken token)
    {
        try
        {
            var snapshot = await _httpClient.GetFromJsonAsync<BitstampOrderBookResponse>(
                "https://www.bitstamp.net/api/v2/order_book/btceur/",
                cancellationToken: token);

            if (snapshot is not null)
            {
                var bids = snapshot.Bids
                    .Select(b => new OrderLevel(ParseDecimal(b[0]), ParseDecimal(b[1])))
                    .ToList();
                var asks = snapshot.Asks
                    .Select(a => new OrderLevel(ParseDecimal(a[0]), ParseDecimal(a[1])))
                    .ToList();

                _state.Update(bids, asks);

                _logger.LogInformation("Loaded initial snapshot ({b} bids, {a} asks)", bids.Count, asks.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load initial snapshot");
        }
    }

    /// <summary>
    /// Handles incoming WebSocket message.
    /// </summary>
    private async Task HandleMessage(string json, CancellationToken token)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("event", out var eventProp))
            {
                var eventType = eventProp.GetString();
                if (eventType == "bts:subscription_succeeded")
                {
                    _logger.LogInformation("WebSocket subscription confirmed");
                    return;
                }

                if (eventType == "data" && doc.RootElement.TryGetProperty("data", out var data))
                {
                    var bids = data.GetProperty("bids")
                        .EnumerateArray()
                        .Select(b => new OrderLevel(ParseDecimal(b[0].GetString()!), ParseDecimal(b[1].GetString()!)))
                        .OrderByDescending(x => x.Price)
                        .ToList();

                    var asks = data.GetProperty("asks")
                        .EnumerateArray()
                        .Select(a => new OrderLevel(ParseDecimal(a[0].GetString()!), ParseDecimal(a[1].GetString()!)))
                        .OrderBy(x => x.Price)
                        .ToList();

                    _state.Update(bids, asks);

                    // Broadcast to SignalR clients
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
                        }, cancellationToken: token);

                        _logger.LogInformation("OrderBook broadcasted ({b} bids, {a} asks)", bids.Count, asks.Count);
                    }

                    // Save snapshot in DB
                    var hash = ComputeSnapshotHash(bids, asks);
                    if (_snapshotTimer.ElapsedMilliseconds > SnapshotMinIntervalMs && _lastSnapshotHash != hash)
                    {
                        _snapshotTimer.Restart();
                        _lastSnapshotHash = hash;

                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOrderBookRepository>();
                        await repo.SaveSnapshotAsync(bids, asks, token);

                        _logger.LogInformation("OrderBook snapshot saved (bids={b}, asks={a})", bids.Count, asks.Count);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message: {json}", json);
        }
    }

    private static decimal ParseDecimal(string value) =>
        decimal.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Computes a hash for snapshot comparison to detect changes.
    /// </summary>
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
