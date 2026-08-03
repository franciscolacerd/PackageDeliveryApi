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

        /// <summary>Lists the authenticated user's deliveries, most recent first.</summary>
        /// <response code="200">The user's deliveries. Empty array when there are none.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetDeliveryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<GetDeliveryModel>>> GetMyDeliveries(CancellationToken cancellationToken)
        {
            var deliveries = await _getDeliveriesService.GetUserDeliveriesAsync(CurrentUserId, cancellationToken);

            return Ok(deliveries);
        }

        /// <summary>Creates a delivery for the authenticated user.</summary>
        /// <remarks>
        /// Validates the request, generates a 15-digit barcode and one package per volume,
        /// and records the initial "created" event.
        /// </remarks>
        /// <response code="200">Delivery created. The response carries the generated barcode.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="422">Validation failed. The response lists the error messages in <c>errors</c>.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateDeliveryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CreateDeliveryResponse), StatusCodes.Status422UnprocessableEntity)]
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
