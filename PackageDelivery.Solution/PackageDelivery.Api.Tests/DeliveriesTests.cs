using System.Net;
using AwesomeAssertions;
using PackageDelivery.Api.Tests._strapper;

namespace PackageDelivery.Api.Tests
{
    public class DeliveriesTests
    {
        [Test]
        public async Task GetDeliveries_WithoutToken_Returns401()
        {
            using var client = ApiClientFactory.GetAnonymousClient();

            var response = await client.GetAsync($"{ApiClientFactory.BaseUrl}/deliveries");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
