using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using PackageDelivery.Api.Tests._strapper;

namespace PackageDelivery.Api.Tests
{
    public class AuthenticationCookieFlowTests
    {
        private static HttpClient RawClient() => new(new HttpClientHandler { UseCookies = false });

        private static Dictionary<string, string> ParseSetCookies(HttpResponseMessage response)
        {
            var jar = new Dictionary<string, string>();
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    var pair = cookie.Split(';')[0];
                    var eq = pair.IndexOf('=');
                    if (eq > 0)
                        jar[pair[..eq]] = pair[(eq + 1)..];
                }
            }
            return jar;
        }

        private static async Task<(HttpResponseMessage response, Dictionary<string, string> cookies)> LoginAsync(HttpClient client)
        {
            var response = await client.PostAsync(ApiClientFactory.LoginUrl, ApiClientFactory.LoginContent());
            return (response, ParseSetCookies(response));
        }

        private static HttpRequestMessage GetWithCookie(string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);
            return request;
        }

        private static HttpRequestMessage RefreshWith(string refreshCookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiClientFactory.RefreshUrl);
            request.Headers.Add("Cookie", $"refresh_token={refreshCookie}");
            return request;
        }

        private static async Task<(string token, string cookieHeader)> GetAntiforgeryAsync(HttpClient client, string accessCookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ApiClientFactory.AntiforgeryUrl);
            request.Headers.Add("Cookie", $"access_token={accessCookie}");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jar = ParseSetCookies(response);
            var cookieHeader = string.Join("; ", jar.Select(kv => $"{kv.Key}={kv.Value}"));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return (doc.RootElement.GetProperty("token").GetString()!, cookieHeader);
        }

        [Test]
        public async Task Login_Returns204AndSetsSecureCookies()
        {
            using var client = RawClient();

            HttpResponseMessage login;
            try { (login, _) = await LoginAsync(client); }
            catch (HttpRequestException) { Assert.Ignore("API not reachable."); return; }

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
        public async Task Account_WithCookieReturns200_WithoutCookieReturns401()
        {
            using var client = RawClient();

            HttpResponseMessage login;
            Dictionary<string, string> cookies;
            try { (login, cookies) = await LoginAsync(client); }
            catch (HttpRequestException) { Assert.Ignore("API not reachable."); return; }

            login.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var url = $"{ApiClientFactory.BaseUrl}/authentication/account";

            var authenticated = await client.SendAsync(GetWithCookie(url, $"access_token={cookies["access_token"]}"));
            authenticated.StatusCode.Should().Be(HttpStatusCode.OK);

            var anonymous = await client.GetAsync(url);
            anonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Refresh_RotatesTokenAndInvalidatesThePreviousOne()
        {
            using var client = RawClient();

            HttpResponseMessage login;
            Dictionary<string, string> cookies;
            try { (login, cookies) = await LoginAsync(client); }
            catch (HttpRequestException) { Assert.Ignore("API not reachable."); return; }

            login.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var oldRefresh = cookies["refresh_token"];

            var rotated = await client.SendAsync(RefreshWith(oldRefresh));
            rotated.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var reuse = await client.SendAsync(RefreshWith(oldRefresh));
            reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Logout_InvalidatesTheSessionOnTheServer()
        {
            using var client = RawClient();

            HttpResponseMessage login;
            Dictionary<string, string> cookies;
            try { (login, cookies) = await LoginAsync(client); }
            catch (HttpRequestException) { Assert.Ignore("API not reachable."); return; }

            login.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var (csrfToken, antiforgeryCookies) = await GetAntiforgeryAsync(client, cookies["access_token"]);

            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, $"{ApiClientFactory.BaseUrl}/authentication/logout");
            logoutRequest.Headers.Add("Cookie", $"access_token={cookies["access_token"]}; {antiforgeryCookies}");
            logoutRequest.Headers.Add("X-CSRF-TOKEN", csrfToken);
            var logout = await client.SendAsync(logoutRequest);
            logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var refreshAfterLogout = await client.SendAsync(RefreshWith(cookies["refresh_token"]));
            refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
