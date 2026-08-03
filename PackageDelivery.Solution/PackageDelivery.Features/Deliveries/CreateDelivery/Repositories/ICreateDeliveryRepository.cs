using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Repositories
{
    public interface ICreateDeliveryRepository
    {
        Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default);
    }
}
