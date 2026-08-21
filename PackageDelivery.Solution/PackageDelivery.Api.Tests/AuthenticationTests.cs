using System.Net;
using System.Text.Json;
using AwesomeAssertions;

namespace PackageDelivery.Api.Tests
{
    public class AuthenticationTests
    {
        [SetUp]
        public void Setup()
        {
            if (!ApiTestHost.IsAvailable)
                Assert.Ignore("SQL Server test container is not available.");
        }

        [Test]
        public async Task Account_WithoutAuthentication_Returns401()
        {
            using var client = ApiTestHost.Raw();

            var response = await client.GetAsync(ApiTestHost.AccountUrl);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Account_AfterLogin_ReturnsCurrentUser()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);

            var request = new HttpRequestMessage(HttpMethod.Get, ApiTestHost.AccountUrl);
            request.Headers.Add("Cookie", $"access_token={cookies["access_token"]}");
            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            root.TryGetProperty("username", out var username).Should().BeTrue();
            username.GetString().Should().Be(ApiTestHost.Username);
            root.TryGetProperty("userId", out var userId).Should().BeTrue();
            userId.GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public async Task Refresh_AfterLogin_IssuesNewCookies()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);

            var request = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.RefreshUrl);
            request.Headers.Add("Cookie", $"refresh_token={cookies["refresh_token"]}");
            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
            setCookies!.Should().Contain(c => c.StartsWith("access_token="));
        }

        [Test]
        public async Task Logout_AfterLogin_ClearsSessionAndAccountReturns401()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);
            var (csrfToken, antiforgeryCookies) = await ApiTestHost.GetAntiforgeryAsync(client, cookies["access_token"]);

            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.LogoutUrl);
            logoutRequest.Headers.Add("Cookie", $"access_token={cookies["access_token"]}; {antiforgeryCookies}");
            logoutRequest.Headers.Add("X-CSRF-TOKEN", csrfToken);
            var logout = await client.SendAsync(logoutRequest);
            logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var refreshAfterLogout = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.RefreshUrl);
            refreshAfterLogout.Headers.Add("Cookie", $"refresh_token={cookies["refresh_token"]}");
            var refresh = await client.SendAsync(refreshAfterLogout);
            refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
