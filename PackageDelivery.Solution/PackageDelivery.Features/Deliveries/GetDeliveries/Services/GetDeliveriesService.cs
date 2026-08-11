using Microsoft.Extensions.Options;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Features.Deliveries.GetDeliveries.Repositories;
using PackageDelivery.Shared.Models;
using PackageDelivery.Shared.Settings;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Services
{
    public class GetDeliveriesService : IGetDeliveriesService
    {
        private readonly PagingSettings _settings;

        private readonly IGetDeliveriesRepository _repository;

        public GetDeliveriesService(IOptions<PagingSettings> settings, IGetDeliveriesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            _settings = settings.Value;
        }

        public Task<PagedResult<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            page = page < 1 ? 1 : page;

            var maxPageSize = _settings.MaxPageSize;

            pageSize = pageSize < 1 ? 20 : (pageSize > maxPageSize ? maxPageSize : pageSize);

            return _repository.GetUserDeliveriesAsync(userId, page, pageSize, cancellationToken);
        }
    }
}