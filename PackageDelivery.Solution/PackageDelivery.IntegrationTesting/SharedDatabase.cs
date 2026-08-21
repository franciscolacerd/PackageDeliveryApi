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

        private static readonly SemaphoreSlim Gate = new(1, 1);

        private static bool _initialized;

        public static bool IsAvailable { get; private set; }

        public static string? ConnectionString { get; private set; }

        public static string? UnavailableReason { get; private set; }

        public static async Task EnsureStartedAsync()
        {
            await Gate.WaitAsync();
            try
            {
                if (_initialized)
                    return;

                _initialized = true;

                try
                {
                    var container = new MsSqlBuilder(Image)
                        .WithName(ContainerName)
                        .WithReuse(true)
                        .Build();

                    await container.StartAsync();

                    var options = new DbContextOptionsBuilder<PackageDeliveryDbContext>()
                        .UseSqlServer(container.GetConnectionString())
                        .Options;

                    await using var context = new PackageDeliveryDbContext(options);
                    await context.Database.MigrateAsync();

                    ConnectionString = container.GetConnectionString();
                    IsAvailable = true;
                }
                catch (Exception exception)
                {
                    IsAvailable = false;
                    ConnectionString = null;
                    UnavailableReason = exception.Message;
                }
            }
            finally
            {
                Gate.Release();
            }
        }
    }
}
