using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace PackageDelivery.Api.Tests._strapper
{
    public static class ApiClientFactory
    {
        private static readonly IConfigurationRoot _config = BuildConfig();
        private static string? _cachedToken;
        private static readonly SemaphoreSlim _tokenLock = new(1, 1);

        public static string RootUrl => _config["ApiSettings:BaseUrl"]!;
        public static string BaseUrl => $"{RootUrl}/api";
        private static string Username => _config["ApiSettings:Username"]!;
        private static string Password => _config["ApiSettings:Password"]!;

        public static HttpClient CreateAnonymousClient() => new();

        public static HttpClient GetAnonymousClient() => CreateAnonymousClient();

        public static FormUrlEncodedContent PasswordGrant() => new(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = Username,
            ["password"] = Password
        });

        public static async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var token = await GetTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static async Task<string> GetTokenAsync()
        {
            if (_cachedToken is not null)
                return _cachedToken;

            await _tokenLock.WaitAsync();
            try
            {
                if (_cachedToken is not null)
                    return _cachedToken;

                using var handler = new HttpClientHandler { UseCookies = false };
                using var bootstrap = new HttpClient(handler);

                var response = await bootstrap.PostAsync($"{RootUrl}/token", PasswordGrant());
                response.EnsureSuccessStatusCode();

                var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
                    ? cookies
                    : Enumerable.Empty<string>();

                var accessCookie = setCookies.FirstOrDefault(c => c.StartsWith("access_token=", StringComparison.Ordinal))
                    ?? throw new InvalidOperationException("O /token não devolveu o cookie 'access_token'.");

                _cachedToken = accessCookie.Split(';')[0]["access_token=".Length..];
                return _cachedToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private static IConfigurationRoot BuildConfig() =>
            new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
    }
}