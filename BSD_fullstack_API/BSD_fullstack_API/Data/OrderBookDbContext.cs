using Microsoft.EntityFrameworkCore;
using BSD_fullstack_API.Models;

namespace BSD_fullstack_API.Data
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
