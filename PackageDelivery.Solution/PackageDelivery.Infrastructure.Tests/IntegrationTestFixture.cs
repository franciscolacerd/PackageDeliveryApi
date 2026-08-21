using PackageDelivery.IntegrationTesting;

namespace PackageDelivery.Infrastructure.Tests
{
    [SetUpFixture]
    public class IntegrationTestFixture
    {
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            var connectionString = await SharedDatabase.EnsureStartedAsync();

            Environment.SetEnvironmentVariable(
                SharedDatabase.ConnectionStringEnvironmentVariable,
                connectionString);
        }
    }
}
