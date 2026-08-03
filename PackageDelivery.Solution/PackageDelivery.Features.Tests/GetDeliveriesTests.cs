using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Features.Tests._strapper;

namespace PackageDelivery.Features.Tests
{
    public class GetDeliveriesTests
    {
        private ServiceProvider _serviceProvider;
        private IGetDeliveriesService _deliveryService;

        [SetUp]
        public void Setup()
        {
            _serviceProvider = Bootstrapper.Bind();
            _deliveryService = _serviceProvider.GetRequiredService<IGetDeliveriesService>();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _serviceProvider.DisposeAsync();
        }

        [Test]
        public async Task GetUserDeliveriesAsync_ReturnsNonNullCollection()
        {
            var result = await _deliveryService.GetUserDeliveriesAsync(userId: 1);

            result.Should().NotBeNull();
        }
    }
}
