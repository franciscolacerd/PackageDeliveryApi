namespace PackageDelivery.Api.Configuration
{
    public static class Antiforgery
    {
        public static IServiceCollection AddAntiforgeryProtection(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "__Host-csrf";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            return services;
        }
    }
}