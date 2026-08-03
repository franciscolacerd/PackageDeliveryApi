using PackageDelivery.Features.Deliveries.GetDeliveries.Models;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Repositories
{
    public interface IGetDeliveriesRepository
    {
        Task<IEnumerable<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, CancellationToken cancellationToken = default);
    }
}
