using Microsoft.EntityFrameworkCore;
using PackageDelivery.Infrastructure.Context;
using Testcontainers.MsSql;

namespace PackageDelivery.IntegrationTesting
{
    public static class SharedDatabase
    {
        public const string ConnectionStringEnvironmentVariable = "ConnectionStrings__PackageDeliveryConnection";

        private const string ContainerName = "packagedelivery-tests-mssql";

        private const string Image = "mcr.microsoft.com/mssql/server:2022-latest";

        private static readonly MsSqlContainer Container = new MsSqlBuilder(Image)
            .WithName(ContainerName)
            .WithReuse(true)
            .Build();

        private static readonly SemaphoreSlim Gate = new(1, 1);

        private static bool _initialized;

        public static async Task<string> EnsureStartedAsync()
        {
            await Gate.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    await Container.StartAsync();

                    var options = new DbContextOptionsBuilder<PackageDeliveryDbContext>()
                        .UseSqlServer(Container.GetConnectionString())
                        .Options;

                    await using var context = new PackageDeliveryDbContext(options);
                    await context.Database.MigrateAsync();

                    _initialized = true;
                }

                return Container.GetConnectionString();
            }
            finally
            {
                Gate.Release();
            }
        }
    }
}
