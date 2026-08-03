using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Infrastructure;

namespace PackageDelivery.Infrastructure.Tests._strapper
{
    public static class Bootstrapper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static ServiceProvider Bind()
        {
            var services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging();

            var configurationRoot = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);

            services.AddInfrastructureServices(configurationRoot);

            services.AddSingleton<IConfiguration>(configurationRoot);

            return services.BuildServiceProvider();
        }
    }
}
