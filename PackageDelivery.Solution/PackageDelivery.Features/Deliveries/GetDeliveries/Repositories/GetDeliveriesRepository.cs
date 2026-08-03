using Microsoft.EntityFrameworkCore;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Infrastructure.Context;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Repositories
{
    public class GetDeliveriesRepository : IGetDeliveriesRepository
    {
        private readonly PackageDeliveryDbContext _context;

        public GetDeliveriesRepository(PackageDeliveryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _context.Deliveries
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedDateUtc)
                .Select(d => new GetDeliveryModel
                {
                    Id = d.Id,
                    BarCode = d.BarCode,
                    SenderName = d.SenderName,
                    ReceiverName = d.ReceiverName,
                    NumberOfVolumes = d.NumberOfVolumes,
                    TotalWeightOfVolumes = d.TotalWeightOfVolumes,
                    CreatedDateUtc = d.CreatedDateUtc
                })
                .ToListAsync(cancellationToken);
        }
    }
}
