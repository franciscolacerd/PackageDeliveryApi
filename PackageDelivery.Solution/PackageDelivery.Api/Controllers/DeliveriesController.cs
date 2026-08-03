using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Api.Controllers
{
    [ApiController]
    [Route("api/deliveries")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableRateLimiting("authenticated")]
    [EnableCors(Policies.CorsPolicy)]
    [Produces("application/json")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IGetDeliveriesService _getDeliveriesService;
        private readonly ICreateDeliveryService _createDeliveryService;

        public DeliveriesController(
            IGetDeliveriesService getDeliveriesService,
            ICreateDeliveryService createDeliveryService)
        {
            _getDeliveriesService = getDeliveriesService ?? throw new ArgumentNullException(nameof(getDeliveriesService));
            _createDeliveryService = createDeliveryService ?? throw new ArgumentNullException(nameof(createDeliveryService));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetDeliveryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<GetDeliveryModel>>> GetMyDeliveries(CancellationToken cancellationToken)
        {
            var deliveries = await _getDeliveriesService.GetUserDeliveriesAsync(CurrentUserId, cancellationToken);

            return Ok(deliveries);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateDeliveryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CreateDeliveryResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CreateDeliveryResponse>> CreateDelivery(
            [FromBody] CreateDeliveryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _createDeliveryService.CreateAsync(request, CurrentUserId, cancellationToken);

            if (!result.Success)
                return UnprocessableEntity(result);

            return Ok(result);
        }

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
