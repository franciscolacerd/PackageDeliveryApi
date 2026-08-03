using PackageDelivery.Features.Deliveries.CreateDelivery.Models;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Services
{
    public interface ICreateDeliveryService
    {
        Task<CreateDeliveryResponse> CreateAsync(CreateDeliveryRequest request, long userId, CancellationToken cancellationToken = default);
    }
}
