using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PackageDelivery.Shared.Exceptions;

namespace PackageDelivery.Api.Configuration
{
    public static class HealthChecks
    {
        public static IHealthChecksBuilder UseHealthChecks<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("PackageDeliveryConnection");
            var baseUrlUri = configuration.GetSection("BaseUrl:Uri").Value;

            if (string.IsNullOrEmpty(baseUrlUri))
                throw new ApiHealthChecksConfigurationException("BaseUrl:Uri not configured in appsettings.json.");

            return services
                .AddHealthChecks()
                .AddSqlServer(
                    connectionString: connectionString ?? string.Empty,
                    healthQuery: "SELECT 1;",
                    name: "Sql Server PackageDelivery",
                    failureStatus: HealthStatus.Degraded)
                .AddDbContextCheck<TContext>()
                .AddUrlGroup(
                    new Uri($"{baseUrlUri}/swagger/index.html"),
                    name: "Swagger",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(30))
                .AddDiskStorageHealthCheck(
                    o => o.AddDrive(@"C:\", 50000),
                    "Server Disk",
                    HealthStatus.Unhealthy);
        }

        public static IApplicationBuilder MapHealthChecks(this IApplicationBuilder builder)
        {
            return builder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
                endpoints.MapHealthChecks("/Healthz", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}
