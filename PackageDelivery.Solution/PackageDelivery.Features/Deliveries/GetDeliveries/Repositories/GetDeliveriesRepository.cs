using Microsoft.EntityFrameworkCore;
using PackageDelivery.Features.Deliveries.GetDeliveries.Models;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Shared.Models;

namespace PackageDelivery.Features.Deliveries.GetDeliveries.Repositories
{
    public class GetDeliveriesRepository : IGetDeliveriesRepository
    {
        private readonly PackageDeliveryDbContext _context;

        public GetDeliveriesRepository(PackageDeliveryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResult<GetDeliveryModel>> GetUserDeliveriesAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Deliveries
                .AsNoTracking()
                .Where(d => d.UserId == userId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(d => d.CreatedDateUtc)
                .ThenByDescending(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return new PagedResult<GetDeliveryModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}