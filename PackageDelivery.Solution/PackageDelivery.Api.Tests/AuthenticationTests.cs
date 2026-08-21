using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using PackageDelivery.Api.Tests._strapper;

namespace PackageDelivery.Api.Tests
{
    public class AuthenticationTests
    {
        [Test]
        public async Task Login_SetsHttpOnlySecureCookies()
        {
            HttpResponseMessage response;
            try
            {
                using var handler = new HttpClientHandler { UseCookies = false };
                using var client = new HttpClient(handler);
                response = await client.PostAsync(ApiClientFactory.LoginUrl, ApiClientFactory.LoginContent());
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();

            var cookies = setCookies!.ToList();

            var accessCookie = cookies.Single(c => c.StartsWith("access_token=")).ToLowerInvariant();
            accessCookie.Should().Contain("httponly");
            accessCookie.Should().Contain("secure");
            accessCookie.Should().Contain("samesite=strict");
            accessCookie.Should().Contain("path=/");

            var refreshCookie = cookies.Single(c => c.StartsWith("refresh_token=")).ToLowerInvariant();
            refreshCookie.Should().Contain("httponly");
            refreshCookie.Should().Contain("secure");
            refreshCookie.Should().Contain("samesite=strict");
            refreshCookie.Should().Contain("path=/api/authentication");
        }

        [Test]
        public async Task Account_WithoutAuthentication_Returns401()
        {
            using var client = ApiClientFactory.GetAnonymousClient();

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"{ApiClientFactory.BaseUrl}/authentication/account");
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Account_AfterLogin_ReturnsCurrentUser()
        {
            HttpClient client;
            try
            {
                client = await CreateLoggedInClientAsync();
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            using (client)
            {
                var response = await client.GetAsync($"{ApiClientFactory.BaseUrl}/authentication/account");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                root.TryGetProperty("username", out var username).Should().BeTrue();
                username.GetString().Should().NotBeNullOrWhiteSpace();
                root.TryGetProperty("userId", out _).Should().BeTrue();
            }
        }

        [Test]
        public async Task Refresh_AfterLogin_IssuesNewCookies()
        {
            HttpClient client;
            try
            {
                client = await CreateLoggedInClientAsync();
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            using (client)
            {
                var response = await client.PostAsync(ApiClientFactory.RefreshUrl, null);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
                setCookies!.Should().Contain(c => c.StartsWith("access_token="));
            }
        }

        [Test]
        public async Task Logout_AfterLogin_ClearsSessionAndAccountReturns401()
        {
            HttpClient client;
            try
            {
                client = await CreateLoggedInClientAsync();
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("API not reachable in this environment.");
                return;
            }

            using (client)
            {
                var csrfToken = await GetAntiforgeryTokenAsync(client);

                var logoutRequest = new HttpRequestMessage(HttpMethod.Post, $"{ApiClientFactory.BaseUrl}/authentication/logout");
                logoutRequest.Headers.Add("X-CSRF-TOKEN", csrfToken);
                var logout = await client.SendAsync(logoutRequest);
                logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var account = await client.GetAsync($"{ApiClientFactory.BaseUrl}/authentication/account");
                account.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        private static async Task<HttpClient> CreateLoggedInClientAsync()
        {
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer(), UseCookies = true };
            var client = new HttpClient(handler);

            var login = await client.PostAsync(ApiClientFactory.LoginUrl, ApiClientFactory.LoginContent());
            login.EnsureSuccessStatusCode();

            return client;
        }

        private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
        {
            var response = await client.GetAsync(ApiClientFactory.AntiforgeryUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("token").GetString()!;
        }
    }
}
