using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Services
{
    public class GetDeliveriesService : IGetDeliveriesService
    {
        private const int MaxPageSize = 100;
        private readonly IGetDeliveriesRepository _repository;

        public GetDeliveriesService(IGetDeliveriesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<PagedResult<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            page = page < 1 ? 1 : page;

            pageSize = pageSize < 1 ? 20 : (pageSize > MaxPageSize ? MaxPageSize : pageSize);

            return _repository.GetUserDeliveriesAsync(userId, page, pageSize, cancellationToken);
        }
    }
}