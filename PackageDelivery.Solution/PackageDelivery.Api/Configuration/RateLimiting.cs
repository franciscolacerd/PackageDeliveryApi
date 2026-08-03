using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PackageDelivery.Api.Configuration
{
    public static class RateLimiting
    {
        public const string Fixed = "fixed";
        public const string Authenticated = "authenticated";

        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter(Fixed, opt =>
                {
                    opt.PermitLimit = configuration.GetValue("RateLimiting:Fixed:PermitLimit", 60);
                    opt.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Fixed:WindowSeconds", 60));
                    opt.QueueLimit = configuration.GetValue("RateLimiting:Fixed:QueueLimit", 0);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.AddTokenBucketLimiter(Authenticated, opt =>
                {
                    opt.TokenLimit = configuration.GetValue("RateLimiting:Authenticated:TokenLimit", 200);
                    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Authenticated:ReplenishSeconds", 10));
                    opt.TokensPerPeriod = configuration.GetValue("RateLimiting:Authenticated:TokensPerPeriod", 20);
                    opt.QueueLimit = configuration.GetValue("RateLimiting:Authenticated:QueueLimit", 5);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.AutoReplenishment = true;
                });

                options.AddPolicy("per-ip", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = configuration.GetValue("RateLimiting:PerIp:PermitLimit", 30),
                            Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:PerIp:WindowSeconds", 60)),
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("RateLimiting");

                    logger.LogWarning("Rate limit exceeded for {RemoteIp} on {Path}",
                        context.HttpContext.Connection.RemoteIpAddress,
                        context.HttpContext.Request.Path);

                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        title = "Too Many Requests",
                        status = 429,
                        detail = "Rate limit exceeded. Try again later.",
                        retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                            ? retryAfter.TotalSeconds
                            : (double?)null
                    }, cancellationToken: cancellationToken);
                };
            });

            return services;
        }
    }
}
