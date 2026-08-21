using System.Net;
using System.Text.Json;
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

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"{ApiClientFactory.BaseUrl}/deliveries");
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetDeliveries_WithToken_ReturnsPagedResult()
        {
            HttpClient client;
            try
            {
                client = await ApiClientFactory.CreateAuthenticatedClientAsync();
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable / authentication unavailable in this environment.");
                return;
            }

            using (client)
            {
                var response = await client.GetAsync($"{ApiClientFactory.BaseUrl}/deliveries?page=1&pageSize=5");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                root.TryGetProperty("items", out var items).Should().BeTrue();
                items.ValueKind.Should().Be(JsonValueKind.Array);
                root.GetProperty("page").GetInt32().Should().Be(1);
                root.GetProperty("pageSize").GetInt32().Should().Be(5);
                root.TryGetProperty("totalCount", out _).Should().BeTrue();
                root.TryGetProperty("totalPages", out _).Should().BeTrue();
            }
        }
    }
}
