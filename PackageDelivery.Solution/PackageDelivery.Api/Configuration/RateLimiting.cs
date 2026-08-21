using System.Security.Claims;
using System.Threading.RateLimiting;

namespace PackageDelivery.Api.Configuration
{
    public static class RateLimiting
    {
        public const string Authenticated = "authenticated";

        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = configuration.GetValue("RateLimiting:PerIp:PermitLimit", 30),
                            Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:PerIp:WindowSeconds", 60)),
                            QueueLimit = 0
                        }));

                options.AddPolicy(Authenticated, context =>
                {
                    var partitionKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";

                    return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = configuration.GetValue("RateLimiting:Authenticated:TokenLimit", 200),
                        ReplenishmentPeriod = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Authenticated:ReplenishSeconds", 10)),
                        TokensPerPeriod = configuration.GetValue("RateLimiting:Authenticated:TokensPerPeriod", 20),
                        QueueLimit = configuration.GetValue("RateLimiting:Authenticated:QueueLimit", 5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
                });

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