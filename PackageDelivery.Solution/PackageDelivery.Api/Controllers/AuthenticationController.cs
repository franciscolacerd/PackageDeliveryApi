using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Api.Middleware;
using PackageDelivery.Features.Authentication.Models;
using PackageDelivery.Features.Authentication.Services;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;
using PackageDelivery.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PackageDelivery.Api.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    [EnableRateLimiting(RateLimiting.Authenticated)]
    [EnableCors(Policies.CorsPolicy)]
    [Produces("application/json")]
    public class AuthenticationController(
        UserManager<AspNetUser> userManager,
        PackageDeliveryDbContext dbContext,
        ITokenService tokenService,
        TokenProviderOptions tokenOptions) : ControllerBase
    {
        /// <summary>Authenticates a user and sets the access and refresh cookies.</summary>
        /// <remarks>On success the tokens are set as HttpOnly cookies and the body is empty.</remarks>
        /// <response code="204">Authenticated; cookies set.</response>
        /// <response code="401">Invalid username or password.</response>
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var pair = await tokenService.IssueForCredentialsAsync(request.Username, request.Password);
            if (pair is null) return Unauthorized();

            WriteAuthenticationCookies(pair);
            return NoContent();
        }

        /// <summary>Rotates the refresh token and re-issues the access and refresh cookies.</summary>
        /// <remarks>Reads the refresh token from its cookie; protected by SameSite=Strict.</remarks>
        /// <response code="204">Refreshed; new cookies set.</response>
        /// <response code="401">Missing, invalid or expired refresh token.</response>
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[AuthenticationCookies.Refresh];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            var pair = await tokenService.RotateRefreshAsync(refreshToken);
            if (pair is null) return Unauthorized();

            WriteAuthenticationCookies(pair);
            return NoContent();
        }

        /// <summary>Returns the profile of the authenticated user.</summary>
        /// <remarks>Reads the username from the token's <c>sub</c> claim and the user id from the name identifier claim.</remarks>
        /// <response code="200">The authenticated user's profile.</response>
        /// <response code="401">Missing or invalid authentication cookie.</response>
        [Authorize]
        [HttpGet("account")]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public IActionResult Account() => Ok(new Account
        {
            Username = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.Identity?.Name,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        /// <summary>Logs the authenticated user out.</summary>
        /// <remarks>Revokes the user's active refresh tokens and clears the access and refresh cookies.</remarks>
        /// <response code="204">The session was cleared.</response>
        /// <response code="401">Missing or invalid authentication cookie.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not null)
            {
                await dbContext.RefreshTokens
                    .Where(t => t.UserId == user.Id && t.RevokedAtUtc == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, DateTime.UtcNow));
            }

            Response.Cookies.Delete(AuthenticationCookies.Access, new CookieOptions { Path = AuthenticationCookies.AccessPath });
            Response.Cookies.Delete(AuthenticationCookies.Refresh, new CookieOptions { Path = AuthenticationCookies.RefreshPath });
            return NoContent();
        }

        /// <summary>Issues an anti-forgery request token for state-changing requests.</summary>
        /// <remarks>
        /// Stores the anti-forgery cookie and returns the request token. The client must send this value
        /// in the <c>X-CSRF-TOKEN</c> header on every unsafe request (POST/PUT/PATCH/DELETE). Call it after login.
        /// </remarks>
        /// <response code="200">The anti-forgery request token.</response>
        [HttpGet("antiforgery/token")]
        [ProducesResponseType(typeof(Tokens), StatusCodes.Status200OK)]
        public IActionResult AntiforgeryToken([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new Tokens { Token = tokens.RequestToken });
        }

        private void WriteAuthenticationCookies(TokenPair pair)
        {
            var secure = tokenOptions.CookieSecure;

            Response.Cookies.Append(AuthenticationCookies.Access, pair.AccessToken,
                AuthenticationCookies.AccessOptions(secure, pair.AccessExpiresAt));
            Response.Cookies.Append(AuthenticationCookies.Refresh, pair.RefreshToken,
                AuthenticationCookies.RefreshOptions(secure, pair.RefreshExpiresAt));
        }
    }
}
