namespace PackageDelivery.Api.Middleware
{
    public static class AuthenticationCookies
    {
        public const string Access = "access_token";
        public const string Refresh = "refresh_token";

        public const string AccessPath = "/";
        public const string RefreshPath = "/api/authentication";

        public static CookieOptions AccessOptions(bool secure, DateTimeOffset expires) => new()
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = AccessPath,
            Expires = expires
        };

        public static CookieOptions RefreshOptions(bool secure, DateTimeOffset expires) => new()
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = RefreshPath,
            Expires = expires
        };
    }
}