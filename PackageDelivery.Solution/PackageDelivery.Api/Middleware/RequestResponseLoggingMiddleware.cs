using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Api.Middleware
{
    public static class RequestResponseLoggingMiddlewareExtension
    {
        public static IApplicationBuilder UseApiLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>(Array.Empty<object>());
        }
    }

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        private static readonly HashSet<string> IgnoredPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/health", "/swagger", "/api/authentication"
        };

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context, PackageDeliveryDbContext logContext)
        {
            if (IgnoredPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var request = await FormatRequestAsync(context.Request);

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            var response = await FormatResponseAsync(context.Response);

            await PersistLogAsync(logContext, context, request, response);

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task PersistLogAsync(PackageDeliveryDbContext logContext, HttpContext context, string request, string response)
        {
            try
            {
                await logContext.ApiRestLogs.AddAsync(new ApiRestLog
                {
                    Host = context.Request.Host.Host,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    Scheme = context.Request.Scheme,
                    RequestBody = request,
                    ResponseBody = response,
                    Origin = context.Request.Headers["Referer"].ToString(),
                    CreatedDate = DateTime.Now,
                    CreatedDateUtc = DateTime.UtcNow
                });

                await logContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving API log - Path: {Path}", context.Request.Path);
            }
        }

        private static Task<string> FormatRequestAsync(HttpRequest request)
        {
            request.EnableBuffering();
            return Task.FromResult($"{request.Scheme} {request.Host}{request.Path} {request.QueryString}");
        }

        private static async Task<string> FormatResponseAsync(HttpResponse response)
        {
            try
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                return $"{response.StatusCode}: {text}";
            }
            catch
            {
                return $"{response.StatusCode}";
            }
        }
    }
}
