using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Repositories
{
    public class CreateDeliveryRepository : ICreateDeliveryRepository
    {
        private readonly PackageDeliveryDbContext _context;

        public CreateDeliveryRepository(PackageDeliveryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default)
        {
            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
