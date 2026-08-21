using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PackageDelivery.IntegrationTesting;

namespace PackageDelivery.Api.Tests
{
    [SetUpFixture]
    public class ApiTestHost
    {
        public const string Username = "cliente@exemplo.pt";
        public const string Password = "Password1!";

        public const string LoginUrl = "/api/authentication/login";
        public const string RefreshUrl = "/api/authentication/refresh";
        public const string AccountUrl = "/api/authentication/account";
        public const string LogoutUrl = "/api/authentication/logout";
        public const string AntiforgeryUrl = "/api/authentication/antiforgery/token";
        public const string DeliveriesUrl = "/api/deliveries";

        public static _strapper.PackageDeliveryApiFactory Factory { get; private set; } = null!;

        public static bool IsAvailable { get; private set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await SharedDatabase.EnsureStartedAsync();

            if (!SharedDatabase.IsAvailable)
                return;

            Environment.SetEnvironmentVariable(
                SharedDatabase.ConnectionStringEnvironmentVariable, SharedDatabase.ConnectionString);
            Environment.SetEnvironmentVariable(
                "Serilog__WriteTo__0__Args__path",
                Path.Combine(Path.GetTempPath(), "packagedelivery-tests", "api-.txt"));

            Factory = new _strapper.PackageDeliveryApiFactory();
            await Factory.SeedUserAsync(Username, Password);

            IsAvailable = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => Factory?.Dispose();

        public static HttpClient Raw() =>
            Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = false,
                AllowAutoRedirect = false
            });

        public static HttpContent LoginContent() => LoginContent(Username, Password);

        public static HttpContent LoginContent(string username, string password) =>
            new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");

        public static Dictionary<string, string> ParseSetCookies(HttpResponseMessage response)
        {
            var jar = new Dictionary<string, string>();
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    var pair = cookie.Split(';')[0];
                    var separator = pair.IndexOf('=');
                    if (separator > 0)
                        jar[pair[..separator]] = pair[(separator + 1)..];
                }
            }
            return jar;
        }

        public static async Task<Dictionary<string, string>> LoginAsync(HttpClient client)
        {
            var response = await client.PostAsync(LoginUrl, LoginContent());
            response.EnsureSuccessStatusCode();
            return ParseSetCookies(response);
        }

        public static async Task<(string token, string cookieHeader)> GetAntiforgeryAsync(HttpClient client, string accessCookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, AntiforgeryUrl);
            request.Headers.Add("Cookie", $"access_token={accessCookie}");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jar = ParseSetCookies(response);
            var cookieHeader = string.Join("; ", jar.Select(entry => $"{entry.Key}={entry.Value}"));

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            return (document.RootElement.GetProperty("token").GetString()!, cookieHeader);
        }
    }
}
