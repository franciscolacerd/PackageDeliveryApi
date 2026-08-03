using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Services
{
    public class GetDeliveriesService : IGetDeliveriesService
    {
        private readonly IGetDeliveriesRepository _repository;

        public GetDeliveriesService(IGetDeliveriesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<IEnumerable<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, CancellationToken cancellationToken = default)
        {
            return _repository.GetUserDeliveriesAsync(userId, cancellationToken);
        }
    }
}
