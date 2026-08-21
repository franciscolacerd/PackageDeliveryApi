using System.Net;
using AwesomeAssertions;

namespace PackageDelivery.Api.Tests
{
    public class AuthenticationCookieFlowTests
    {
        [SetUp]
        public void Setup()
        {
            if (!ApiTestHost.IsAvailable)
                Assert.Ignore("SQL Server test container is not available.");
        }

        private static HttpRequestMessage GetWithCookie(string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);
            return request;
        }

        private static HttpRequestMessage RefreshWith(string refreshCookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.RefreshUrl);
            request.Headers.Add("Cookie", $"refresh_token={refreshCookie}");
            return request;
        }

        [Test]
        public async Task Login_Returns204AndSetsSecureCookies()
        {
            using var client = ApiTestHost.Raw();

            var login = await client.PostAsync(ApiTestHost.LoginUrl, ApiTestHost.LoginContent());

            login.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await login.Content.ReadAsStringAsync()).Should().BeEmpty();

            var setCookies = login.Headers.GetValues("Set-Cookie").ToList();
            setCookies.Should().Contain(c =>
                c.StartsWith("access_token=")
                && c.Contains("httponly", StringComparison.OrdinalIgnoreCase)
                && c.Contains("secure", StringComparison.OrdinalIgnoreCase)
                && c.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));
            setCookies.Should().Contain(c =>
                c.StartsWith("refresh_token=")
                && c.Contains("path=/api/authentication", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task Login_WithInvalidCredentials_Returns401()
        {
            using var client = ApiTestHost.Raw();

            var login = await client.PostAsync(ApiTestHost.LoginUrl, ApiTestHost.LoginContent(ApiTestHost.Username, "WrongPassword1!"));

            login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Account_WithCookieReturns200_WithoutCookieReturns401()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);

            var authenticated = await client.SendAsync(GetWithCookie(ApiTestHost.AccountUrl, $"access_token={cookies["access_token"]}"));
            authenticated.StatusCode.Should().Be(HttpStatusCode.OK);

            var anonymous = await client.GetAsync(ApiTestHost.AccountUrl);
            anonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Refresh_RotatesTokenAndInvalidatesThePreviousOne()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);
            var oldRefresh = cookies["refresh_token"];

            var rotated = await client.SendAsync(RefreshWith(oldRefresh));
            rotated.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var reuse = await client.SendAsync(RefreshWith(oldRefresh));
            reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Logout_InvalidatesTheSessionOnTheServer()
        {
            using var client = ApiTestHost.Raw();

            var cookies = await ApiTestHost.LoginAsync(client);

            var (csrfToken, antiforgeryCookies) = await ApiTestHost.GetAntiforgeryAsync(client, cookies["access_token"]);

            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, ApiTestHost.LogoutUrl);
            logoutRequest.Headers.Add("Cookie", $"access_token={cookies["access_token"]}; {antiforgeryCookies}");
            logoutRequest.Headers.Add("X-CSRF-TOKEN", csrfToken);
            var logout = await client.SendAsync(logoutRequest);
            logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var refreshAfterLogout = await client.SendAsync(RefreshWith(cookies["refresh_token"]));
            refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
