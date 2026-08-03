using PackageDelivery.Shared.Models;

namespace PackageDelivery.Api.Configuration
{
    public static class Cors
    {
        public static void AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["BaseUrl:Uri"]?.TrimEnd('/') ?? string.Empty;

            services.AddCors(options =>
            {
                options.AddPolicy(Policies.CorsPolicy, builder =>
                {
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        builder
                            .WithOrigins(baseUrl)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
                    }
                    else
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
                    }
                });
            });
        }
    }
}
