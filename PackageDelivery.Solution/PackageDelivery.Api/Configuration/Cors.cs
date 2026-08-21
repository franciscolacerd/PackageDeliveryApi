using PackageDelivery.Shared.Models;

namespace PackageDelivery.Api.Configuration
{
    public static class Cors
    {
        public static void AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(Policies.CorsPolicy, builder =>
                {
                    if (origins.Length == 0)
                        return;

                    builder
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
                });
            });
        }
    }
}