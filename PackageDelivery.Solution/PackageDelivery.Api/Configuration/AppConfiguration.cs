namespace PackageDelivery.Api.Configuration
{
    public static class AppConfiguration
    {
        public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
        {
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder;
        }
    }
}
