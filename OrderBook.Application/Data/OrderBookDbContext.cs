using Microsoft.EntityFrameworkCore;
using OrderBook.Application.Models;

namespace OrderBook.Application.Data;

/// <summary>
/// Entity Framework Core DbContext for OrderBook application.
/// </summary>
public class OrderBookDbContext : DbContext
{
    public OrderBookDbContext(DbContextOptions<OrderBookDbContext> options)
        : base(options)
    {
    }

    /// <summary> DbSet of order book snapshots. </summary>
    public DbSet<OrderBookSnapshot> OrderBookSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderBookSnapshot>()
            .HasIndex(x => x.Timestamp);
    }
}
