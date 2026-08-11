using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Repositories
{
    public interface IGetDeliveriesRepository
    {
        Task<PagedResult<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}