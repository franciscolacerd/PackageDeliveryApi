using Microsoft.Extensions.Logging;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _repository;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            IDeliveryRepository repository,
            ILogger<DeliveryService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching deliveries for user {UserId}", userId);
            var deliveries = await _repository.GetUserDeliveriesAsync(userId, cancellationToken);
            _logger.LogInformation("Fetched {Count} deliveries for user {UserId}", deliveries.Count(), userId);
            return deliveries;
        }
    }
}
