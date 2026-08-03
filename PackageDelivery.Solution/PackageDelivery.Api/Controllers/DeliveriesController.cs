using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Api.Controllers
{
    [ApiController]
    [Route("api/deliveries")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableRateLimiting("authenticated")]
    [Produces("application/json")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveriesController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
        }

        [HttpGet]
        [EnableCors(Policies.CorsPolicy)]
        [ProducesResponseType(typeof(IEnumerable<GetDeliveryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<GetDeliveryModel>>> GetMyDeliveries(CancellationToken cancellationToken)
        {
            var userId = long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

            var deliveries = await _deliveryService.GetUserDeliveriesAsync(userId, cancellationToken);

            return Ok(deliveries);
        }
    }
}
