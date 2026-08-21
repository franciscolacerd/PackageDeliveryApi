using System.Net;
using AwesomeAssertions;
using PackageDelivery.Api.Tests._strapper;

namespace PackageDelivery.Api.Tests
{
    // E2E do fluxo de auth por cookies, contra a API viva (ApiSettings:BaseUrl, https:7280).
    // Cookies tratados à mão (UseCookies=false): lê-se o Set-Cookie e reenvia-se como header Cookie,
    // por isso funciona em HTTP ou HTTPS (ignora a flag Secure). Assert.Ignore quando a API não responde.
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
        public async Task Login_devolve_204_e_poe_cookies_seguros()
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
        public async Task Account_com_cookie_devolve_200_e_sem_cookie_401()
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
        public async Task Refresh_roda_o_token_e_o_antigo_deixa_de_valer()
        {
            using var client = RawClient();

            HttpResponseMessage login;
            Dictionary<string, string> cookies;
            try { (login, cookies) = await LoginAsync(client); }
            catch (HttpRequestException) { Assert.Ignore("API not reachable."); return; }

            login.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var oldRefresh = cookies["refresh_token"];

            var rotated = await client.SendAsync(RefreshWith(oldRefresh));
            rotated.StatusCode.Should().Be(HttpStatusCode.NoContent);       // novo par emitido nos cookies

            var reuse = await client.SendAsync(RefreshWith(oldRefresh));
            reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);      // 1 por user: o antigo foi substituído
        }

        [Test]
        public async Task Logout_invalida_a_sessao_no_servidor()
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

            // o logout removeu o refresh da BD → reutilizá-lo passa a falhar
            var refreshAfterLogout = await client.SendAsync(RefreshWith(cookies["refresh_token"]));
            refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
