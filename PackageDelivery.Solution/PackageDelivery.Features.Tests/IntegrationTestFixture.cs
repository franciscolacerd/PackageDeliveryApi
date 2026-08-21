using PackageDelivery.IntegrationTesting;

namespace PackageDelivery.Features.Tests
{
    [SetUpFixture]
    public class IntegrationTestFixture
    {
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            await SharedDatabase.EnsureStartedAsync();

            if (SharedDatabase.IsAvailable)
                Environment.SetEnvironmentVariable(
                    SharedDatabase.ConnectionStringEnvironmentVariable,
                    SharedDatabase.ConnectionString);
        }
    }
}
