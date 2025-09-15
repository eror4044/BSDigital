using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OrderBook.Application.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderBookDbContext>
    {
        public OrderBookDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("OrderBookDb");

            var optionsBuilder = new DbContextOptionsBuilder<OrderBookDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new OrderBookDbContext(optionsBuilder.Options);
        }
    }
}
