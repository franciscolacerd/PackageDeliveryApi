using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;
using PackageDelivery.Shared.Models;
using PackageDelivery.Shared.Policies;
using Polly;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace PackageDelivery.Api.Middleware
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly ILogger _logger;
        protected Polly.Retry.AsyncRetryPolicy AsyncPolicy { get; }

        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            ILogger<TokenProviderMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
            AsyncPolicy = Policy.Handle<System.Data.Common.DbException>().Or<TimeoutException>().WaitAndRetryAsync(ExceptionJitter.Get5RetryCount());
        }

        public Task Invoke(HttpContext context, UserManager<AspNetUser> userManager, PackageDeliveryDbContext dbContext)
        {
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
                return _next(context);

            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context, userManager, dbContext);
        }

        private async Task GenerateToken(HttpContext context, UserManager<AspNetUser> userManager, PackageDeliveryDbContext dbContext)
        {
            var sw = Stopwatch.StartNew();
            var grantType = context.Request.Form["grant_type"].ToString();

            if (string.IsNullOrEmpty(grantType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid grant_type.");
                return;
            }

            if (grantType == "password")
            {
                await HandlePasswordGrant(context, userManager, sw);
            }
            else if (grantType == "refresh_token")
            {
                await HandleRefreshTokenGrant(context, userManager, dbContext, sw);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid grant_type.");
            }
        }

        private async Task HandlePasswordGrant(HttpContext context, UserManager<AspNetUser> userManager, Stopwatch sw)
        {
            var username = context.Request.Form["username"].ToString();
            var password = context.Request.Form["password"].ToString();

            var (identity, user) = await GetIdentityAsync(username, password, userManager);

            if (identity == null || user == null)
            {
                sw.Stop();
                _logger.LogWarning("Failed authentication attempt for user {Username} in {Duration}ms",
                    username, sw.Elapsed.TotalMilliseconds);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid username or password.");
                return;
            }

            var (accessToken, refreshToken) = await GenerateTokenPairAsync(user, identity, userManager);

            if (string.IsNullOrEmpty(accessToken))
            {
                sw.Stop();
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Token generation failed.");
                return;
            }

            sw.Stop();
            _logger.LogInformation("User {Username} authenticated successfully and token issued in {Duration}ms",
                username, sw.Elapsed.TotalMilliseconds);

            var secure = _options.CookieSecure;

            context.Response.Cookies.Append(AuthenticationCookies.Access, accessToken, AuthenticationCookies.AccessOptions(secure, DateTimeOffset.UtcNow.Add(_options.Expiration)));
            context.Response.Cookies.Append(AuthenticationCookies.Refresh, refreshToken, AuthenticationCookies.RefreshOptions(secure, DateTimeOffset.UtcNow.Add(_options.RefreshTokenExpiration)));
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        }

        private async Task HandleRefreshTokenGrant(HttpContext context, UserManager<AspNetUser> userManager, PackageDeliveryDbContext dbContext, Stopwatch sw)
        {
            var refreshToken = context.Request.Cookies[AuthenticationCookies.Refresh];

            if (string.IsNullOrEmpty(refreshToken))
                refreshToken = context.Request.Form["refresh_token"].ToString();

            try
            {
                var userToken = await dbContext.UserTokens
                    .FirstOrDefaultAsync(t => t.LoginProvider == "RefreshTokenProvider"
                        && t.Name == "RefreshToken"
                        && t.Value == refreshToken);

                if (userToken == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Invalid or expired refresh token attempt in {Duration}ms", sw.Elapsed.TotalMilliseconds);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid or expired refresh token.");
                    return;
                }

                var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userToken.UserId);

                if (user == null || !user.Active)
                {
                    sw.Stop();
                    _logger.LogWarning("Invalid refresh token or inactive user in {Duration}ms", sw.Elapsed.TotalMilliseconds);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid or expired refresh token.");
                    return;
                }

                var identity = new ClaimsIdentity(new GenericIdentity(user.UserName!, "Token"));
                var (accessToken, newRefreshToken) = await GenerateTokenPairAsync(user, identity, userManager);

                if (string.IsNullOrEmpty(accessToken))
                {
                    sw.Stop();
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Token generation failed.");
                    return;
                }

                sw.Stop();
                _logger.LogInformation("User {Username} token refreshed successfully in {Duration}ms",
                    user.UserName, sw.Elapsed.TotalMilliseconds);

                var secure = _options.CookieSecure;

                context.Response.Cookies.Append(AuthenticationCookies.Access, accessToken, AuthenticationCookies.AccessOptions(secure, DateTimeOffset.UtcNow.Add(_options.Expiration)));
                context.Response.Cookies.Append(AuthenticationCookies.Refresh, newRefreshToken, AuthenticationCookies.RefreshOptions(secure, DateTimeOffset.UtcNow.Add(_options.RefreshTokenExpiration)));
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error processing refresh token");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error.");
            }
        }

        private async Task<(string accessToken, string refreshToken)> GenerateTokenPairAsync(
            AspNetUser user, ClaimsIdentity identity, UserManager<AspNetUser> userManager)
        {
            var now = DateTime.UtcNow;
            var claims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            identity.AddClaims(claims);

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: identity.Claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refreshToken = GenerateRefreshToken();

            await userManager.SetAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken", refreshToken);

            return (encodedJwt, refreshToken);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToHexString(randomNumber);
        }

        private async Task<(ClaimsIdentity?, AspNetUser?)> GetIdentityAsync(
            string username, string password, UserManager<AspNetUser> userManager)
        {
            try
            {
                return await AsyncPolicy.ExecuteAsync(async () =>
                {
                    var user = await userManager.Users
                        .Where(x => x.UserName!.Equals(username))
                        .FirstOrDefaultAsync();

                    if (user == null) return (null, null);

                    var result = await userManager.CheckPasswordAsync(user, password);
                    if (!result) return (null, null);

                    if (!user.Active) return (null, null);

                    return ((ClaimsIdentity?)new ClaimsIdentity(new GenericIdentity(username, "Token")), (AspNetUser?)user);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return (null, null);
        }
    }
}