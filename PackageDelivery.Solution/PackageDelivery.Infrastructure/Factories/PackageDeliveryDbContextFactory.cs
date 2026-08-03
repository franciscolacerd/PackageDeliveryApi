using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PackageDelivery.Infrastructure.Context;

namespace PackageDelivery.Infrastructure.Factories
{
    public class PackageDeliveryDbContextFactory : IDesignTimeDbContextFactory<PackageDeliveryDbContext>
    {
        public PackageDeliveryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PackageDeliveryDbContext>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("PackageDeliveryConnection"),
                options => options
                    .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), null)
                    .CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            return new PackageDeliveryDbContext(optionsBuilder.Options);
        }
    }
}
