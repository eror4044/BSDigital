using Microsoft.AspNetCore.SignalR;

namespace BSD_fullstack_API.Hubs;

public class OrderBookHub : Hub
{
    // Позже будем вызывать Clients.All.SendAsync("orderbook:update", data);
}
