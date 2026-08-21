namespace PackageDelivery.Features.Authentication.Models
{
    public sealed record TokenPair(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessExpiresAt,
        DateTimeOffset RefreshExpiresAt);
}
