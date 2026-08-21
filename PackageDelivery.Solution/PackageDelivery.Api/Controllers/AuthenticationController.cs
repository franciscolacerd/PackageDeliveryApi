using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Api.Middleware;
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
    public class AuthenticationController(UserManager<AspNetUser> userManager) : ControllerBase
    {
        [Authorize]
        [HttpGet("account")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        public IActionResult Account() => Ok(new Account
        {
            Username = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.Identity?.Name,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        });

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is not null)
                await userManager.RemoveAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken");

            Response.Cookies.Delete(AuthenticationCookies.Access, new CookieOptions { Path = "/" });
            Response.Cookies.Delete(AuthenticationCookies.Refresh, new CookieOptions { Path = "/token" });
            return NoContent();
        }
    }
}