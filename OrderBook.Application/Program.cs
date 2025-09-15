using Microsoft.EntityFrameworkCore;
using OrderBook.Application.Data;
using OrderBook.Application.Data.Repositories;
using OrderBook.Application.Hubs;
using OrderBook.Application.Interfaces;
using OrderBook.Application.Models;
using OrderBook.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<BitstampService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var env = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<OrderBookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderBookDb")));
builder.Services.AddScoped<IOrderBookRepository, OrderBookRepository>();
builder.Services.AddSingleton<IOrderBookState, OrderBookState>();
builder.Services.AddSingleton<IQuoteService, QuoteService>();
builder.Services.AddSingleton<OrderBookCache>();
builder.Services.AddHostedService<OrderBookRetentionService>();
builder.Services.AddHttpClient();

var app = builder.Build();


app.UseCors("AllowVueClient");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderBookHub>("/hubs/orderbook");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
