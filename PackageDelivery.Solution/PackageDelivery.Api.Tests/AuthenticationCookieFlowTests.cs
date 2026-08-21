using System.Net;
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
            var response = await client.PostAsync($"{ApiClientFactory.RootUrl}/token", ApiClientFactory.PasswordGrant());
            return (response, ParseSetCookies(response));
        }

        private static HttpRequestMessage GetWithCookie(string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);
            return request;
        }

        private static HttpRequestMessage PostWithCookie(string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Cookie", cookie);
            return request;
        }

        private static HttpRequestMessage RefreshWith(string refreshCookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiClientFactory.RootUrl}/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "refresh_token" })
            };
            request.Headers.Add("Cookie", $"refresh_token={refreshCookie}");
            return request;
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
                && c.Contains("path=/token", StringComparison.OrdinalIgnoreCase));
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

            var logout = await client.SendAsync(PostWithCookie(
                $"{ApiClientFactory.BaseUrl}/authentication/logout", $"access_token={cookies["access_token"]}"));
            logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var refreshAfterLogout = await client.SendAsync(RefreshWith(cookies["refresh_token"]));
            refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
