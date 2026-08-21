using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Api.Middleware;
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
    public class AuthenticationController(UserManager<AspNetUser> userManager, PackageDeliveryDbContext dbContext) : ControllerBase
    {
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
        /// <remarks>Removes the stored refresh token and clears the access and refresh cookies.</remarks>
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

            Response.Cookies.Delete(AuthenticationCookies.Access, new CookieOptions { Path = "/" });
            Response.Cookies.Delete(AuthenticationCookies.Refresh, new CookieOptions { Path = "/token" });
            return NoContent();
        }

        /// <summary>Issues an anti-forgery request token for state-changing requests.</summary>
        /// <remarks>
        /// Stores the anti-forgery cookie and returns the request token. The client must send this value
        /// in the <c>X-CSRF-TOKEN</c> header on every unsafe request (POST/PUT/PATCH/DELETE). Call it after login.
        /// </remarks>
        /// <response code="200">The anti-forgery request token.</response>
        [HttpGet("antiforgery/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Tokens), StatusCodes.Status200OK)]
        public IActionResult AntiforgeryToken([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new Tokens { Token = tokens.RequestToken });
        }
    }
}