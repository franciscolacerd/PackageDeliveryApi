using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;

namespace PackageDelivery.Api.Tests
{
    public class DeliveriesTests
    {
        [SetUp]
        public void Setup()
        {
            if (!ApiTestHost.IsAvailable)
                Assert.Ignore("SQL Server test container is not available.");
        }

        [Test]
        public async Task GetDeliveries_WithoutToken_Returns401()
        {
            using var client = ApiTestHost.Raw();

            var response = await client.GetAsync(ApiTestHost.DeliveriesUrl);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetDeliveries_WithToken_ReturnsPagedResult()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiTestHost.DeliveriesUrl}?page=1&pageSize=5");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies["access_token"]);
            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            root.TryGetProperty("items", out var items).Should().BeTrue();
            items.ValueKind.Should().Be(JsonValueKind.Array);
            root.GetProperty("page").GetInt32().Should().Be(1);
            root.GetProperty("pageSize").GetInt32().Should().Be(5);
            root.TryGetProperty("totalCount", out _).Should().BeTrue();
            root.TryGetProperty("totalPages", out _).Should().BeTrue();
        }

        [Test]
        public async Task CreateDelivery_WithBusinessValidationError_Returns422ValidationProblemDetails()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);
            var (csrfToken, antiforgeryCookies) = await ApiTestHost.GetAntiforgeryAsync(client, cookies["access_token"]);

            var payload = InvalidBusinessPayload();
            var request = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.DeliveriesUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Cookie", $"access_token={cookies["access_token"]}; {antiforgeryCookies}");
            request.Headers.Add("X-CSRF-TOKEN", csrfToken);

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(422);
            root.TryGetProperty("title", out _).Should().BeTrue();
            root.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Object);
            errors.TryGetProperty("Details.NumberOfVolumes", out _).Should().BeTrue();
        }

        private static object InvalidBusinessPayload() => new
        {
            details = new
            {
                clientReference = "REF-001",
                numberOfVolumes = 0,
                totalWeightOfVolumes = 4.5m,
                instructions = "Handle with care",
                preferentialPeriod = "09:00-13:00"
            },
            sender = ValidParty("Acme Warehouse"),
            receiver = ValidParty("Jane Receiver"),
            attributes = new { pod = true, sameDay = false, cashOnDelivery = false }
        };

        private static object ValidParty(string name) => new
        {
            name,
            contact = new { name, phoneNumber = "912345678", email = "party@example.com" },
            address = new
            {
                addressLine = "Rua A, 1",
                place = "Lisboa",
                zipCode = "1000-001",
                zipCodePlace = "Lisboa",
                countryCode = "PT"
            }
        };
    }
}
