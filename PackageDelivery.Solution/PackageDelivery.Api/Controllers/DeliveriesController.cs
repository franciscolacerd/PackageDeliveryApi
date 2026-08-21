using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PackageDelivery.Api.Configuration;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Shared.Models;
using System.Security.Claims;

namespace PackageDelivery.Api.Controllers
{
    [ApiController]
    [Route("api/deliveries")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableRateLimiting(RateLimiting.Authenticated)]
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

        /// <summary>Lists the authenticated user's deliveries, most recent first, paginated.</summary>
        /// <param name="page">1-based page number. Defaults to 1; values below 1 are clamped to 1.</param>
        /// <param name="pageSize">Page size. Defaults to 20; capped at 100.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <response code="200">A page of the user's deliveries. The <c>items</c> array is empty when there are none.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GetDeliveryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResult<GetDeliveryModel>>> GetUserDeliveriesAsync(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                CancellationToken cancellationToken = default)
        {
            var deliveries = await _getDeliveriesService.GetUserDeliveriesAsync(CurrentUserId, page, pageSize, cancellationToken);

            return Ok(deliveries);
        }

        /// <summary>Creates a delivery for the authenticated user.</summary>
        /// <remarks>
        /// Validates the request, generates a 15-digit barcode and one package per volume,
        /// and records the initial "created" event.
        /// </remarks>
        /// <response code="200">Delivery created. The response carries the generated barcode.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="422">Validation failed. Returns a <c>ValidationProblemDetails</c> with the validation failures keyed by field in <c>errors</c>.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateDeliveryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateDeliveryResponse>> CreateDeliveryAsync(
            [FromBody] CreateDeliveryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _createDeliveryService.CreateAsync(request, CurrentUserId, cancellationToken);

            if (!result.Success)
            {
                var problemDetails = new ValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "One or more validation errors occurred."
                };
                return UnprocessableEntity(problemDetails);
            }

            return Ok(result);
        }

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}