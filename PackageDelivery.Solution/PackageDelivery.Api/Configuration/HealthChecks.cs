using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PackageDelivery.Shared.Exceptions;

namespace PackageDelivery.Api.Configuration
{
    public static class HealthChecks
    {
        public static IHealthChecksBuilder UseHealthChecks<TContext>(
            this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("PackageDeliveryConnection");
            var baseUrlUri = configuration.GetSection("BaseUrl:Uri").Value;

            if (string.IsNullOrEmpty(baseUrlUri))
                throw new ApiHealthChecksConfigurationException("BaseUrl:Uri not configured.");

            var hc = services
                .AddHealthChecks()
                .AddSqlServer(connectionString ?? string.Empty, healthQuery: "SELECT 1;",
                    name: "Sql Server PackageDelivery", failureStatus: HealthStatus.Degraded)
                .AddDbContextCheck<TContext>()
                .AddUrlGroup(new Uri($"{baseUrlUri}/swagger/index.html"),
                    name: "Swagger", failureStatus: HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(30));

            if (OperatingSystem.IsWindows())
                hc.AddDiskStorageHealthCheck(o => o.AddDrive(@"C:\", 50000), "Server Disk", HealthStatus.Unhealthy);

            return hc;
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