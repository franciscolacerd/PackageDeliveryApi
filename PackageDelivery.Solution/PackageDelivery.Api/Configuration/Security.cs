using Microsoft.Net.Http.Headers;

namespace PackageDelivery.Api.Configuration
{
    public static class Security
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services) => services;

        public static IApplicationBuilder UseSecurity(this IApplicationBuilder app)
        {
            var policies = new HeaderPolicyCollection()
                .AddFrameOptionsSameOrigin()
                .AddContentTypeOptionsNoSniff()
                .AddReferrerPolicyNoReferrer()
                .RemoveServerHeader()
                .AddStrictTransportSecurityMaxAgeIncludeSubDomainsAndPreload(maxAgeInSeconds: 60 * 60 * 24 * 365)
                .AddContentSecurityPolicy(builder =>
                {
                    builder.AddDefaultSrc().Self();
                    builder.AddObjectSrc().None();
                    builder.AddFrameAncestors().Self();
                })
                .AddCustomHeader("X-XSS-Protection", "0")
                .AddCustomHeader("X-Robots-Tag", "noindex, nofollow");

            app.UseSecurityHeaders(policies);

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var name = ctx.File.Name;
                    if (name.EndsWith(".css") || name.EndsWith(".js") ||
                        name.EndsWith(".png") || name.EndsWith(".jpg"))
                    {
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, "public, max-age=31536000");
                    }
                    else
                    {
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, "no-store, no-cache, must-revalidate");
                    }
                }
            });

            return app;
        }
    }
}