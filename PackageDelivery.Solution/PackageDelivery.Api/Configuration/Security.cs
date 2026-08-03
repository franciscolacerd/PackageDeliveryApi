using Microsoft.Net.Http.Headers;

namespace PackageDelivery.Api.Configuration
{
    public static class Security
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            return services;
        }

        public static IApplicationBuilder UseSecurity(this IApplicationBuilder app)
        {
            app.UseXRobotsTag(options => options.NoIndex().NoFollow());
            app.UseXDownloadOptions();
            app.UseXContentTypeOptions();
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.SameOrigin());
            app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains().Preload());
            app.UseReferrerPolicy(opts => opts.NoReferrer());

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js") ||
                        ctx.File.Name.EndsWith(".png") || ctx.File.Name.EndsWith(".jpg"))
                    {
                        ctx.Context.Response.Headers.Append(HeaderNames.CacheControl, "public, max-age=31536000");
                    }
                    else
                    {
                        app.UseNoCacheHttpHeaders();
                    }
                }
            });

            app.UseRedirectValidation();

            return app;
        }
    }
}
