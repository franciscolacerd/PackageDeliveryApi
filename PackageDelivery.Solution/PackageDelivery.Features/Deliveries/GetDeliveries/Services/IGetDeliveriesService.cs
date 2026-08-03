using PackageDelivery.Features.Deliveries.GetDeliveries.Models;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Services
{
    public interface IGetDeliveriesService
    {
        Task<IEnumerable<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, CancellationToken cancellationToken = default);
    }
}
