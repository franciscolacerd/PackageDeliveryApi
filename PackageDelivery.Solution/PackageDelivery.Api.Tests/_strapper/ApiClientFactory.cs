using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

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

                using var bootstrap = new HttpClient();
                var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = Username,
                    ["password"] = Password
                });

                var response = await bootstrap.PostAsync($"{RootUrl}/token", formContent);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                _cachedToken = doc.RootElement.GetProperty("access_token").GetString()!;
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
