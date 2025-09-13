using Microsoft.EntityFrameworkCore;
using OrderBook.Application.Models;

namespace OrderBook.Application.Data
{
    public class OrderBookDbContext : DbContext
    {
        public OrderBookDbContext(DbContextOptions<OrderBookDbContext> options)
            : base(options)
        {
        }

        public DbSet<OrderBookSnapshot> OrderBookSnapshots { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderBookSnapshot>()
                .HasIndex(x => x.Timestamp);
        }
    }
}
