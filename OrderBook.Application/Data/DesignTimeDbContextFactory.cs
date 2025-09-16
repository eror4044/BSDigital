using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderBook.Application.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderBookDbContext>
    {
        public OrderBookDbContext CreateDbContext(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("OrderBookDb")
                ?? throw new InvalidOperationException("Connection string 'OrderBookDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<OrderBookDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new OrderBookDbContext(optionsBuilder.Options);
        }
    }
}
