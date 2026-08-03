using Microsoft.IdentityModel.Tokens;

namespace PackageDelivery.Shared.Models
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/token";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(8);
        public SigningCredentials SigningCredentials { get; set; } = null!;
        public string CookieName { get; set; } = "access_token";
        public TimeSpan RefreshTokenExpiration { get; set; } = TimeSpan.FromDays(30);
    }
}
