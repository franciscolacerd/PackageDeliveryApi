using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Deliveries.GetDeliveries.Services;
using PackageDelivery.Features.Tests._strapper;
using PackageDelivery.Infrastructure.Context;

namespace PackageDelivery.Features.Tests.Deliveries.GetDeliveries
{
    public class GetDeliveriesServiceTests
    {
        private ServiceProvider _serviceProvider = null!;
        private IGetDeliveriesService _service = null!;
        private ICreateDeliveryService _createService = null!;
        private PackageDeliveryDbContext _context = null!;
        private readonly List<string> _createdBarCodes = new();

        [SetUp]
        public void Setup()
        {
            _serviceProvider = Bootstrapper.Bind();
            _service = _serviceProvider.GetRequiredService<IGetDeliveriesService>();
            _createService = _serviceProvider.GetRequiredService<ICreateDeliveryService>();
            _context = _serviceProvider.GetRequiredService<PackageDeliveryDbContext>();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_createdBarCodes.Count > 0)
            {
                var toDelete = await _context.Deliveries
                    .Where(d => _createdBarCodes.Contains(d.BarCode))
                    .ToListAsync();

                _context.Deliveries.RemoveRange(toDelete);
                await _context.SaveChangesAsync();
            }

            await _context.DisposeAsync();
            await _serviceProvider.DisposeAsync();
        }

        [Test]
        public async Task GetUserDeliveriesAsync_returns_the_users_created_delivery()
        {
            var userId = Random.Shared.NextInt64(1_000_000_000, long.MaxValue);

            var created = await _createService.CreateAsync(ValidRequest(), userId);
            _createdBarCodes.Add(created.BarCode!);

            created.Success.Should().BeTrue();

            var persistedUserId = await _context.Deliveries
                .AsNoTracking()
                .Where(d => d.BarCode == created.BarCode)
                .Select(d => (long?)d.UserId)
                .SingleOrDefaultAsync();

            persistedUserId.Should().Be(userId);

            var countByUser = await _context.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.UserId == userId);

            countByUser.Should().Be(1);

            var result = await _service.GetUserDeliveriesAsync(userId, 1, 20);

            var items = result.Items;

            items.Should().ContainSingle();
            items[0].BarCode.Should().Be(created.BarCode);
            items[0].NumberOfVolumes.Should().Be(2);

            result.TotalCount.Should().Be(1);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.TotalPages.Should().Be(1);
            result.HasPrevious.Should().BeFalse();
            result.HasNext.Should().BeFalse();
        }

        [Test]
        public async Task GetUserDeliveriesAsync_paginates_when_there_is_more_than_one_page()
        {
            var userId = Random.Shared.NextInt64(1_000_000_000, long.MaxValue);

            for (var i = 0; i < 3; i++)
            {
                var created = await _createService.CreateAsync(ValidRequest(), userId);
                _createdBarCodes.Add(created.BarCode!);
            }

            var firstPage = await _service.GetUserDeliveriesAsync(userId, 1, 2);

            firstPage.Items.Should().HaveCount(2);
            firstPage.TotalCount.Should().Be(3);
            firstPage.TotalPages.Should().Be(2);
            firstPage.HasPrevious.Should().BeFalse();
            firstPage.HasNext.Should().BeTrue();

            var secondPage = await _service.GetUserDeliveriesAsync(userId, 2, 2);

            secondPage.Items.Should().ContainSingle();
            secondPage.HasPrevious.Should().BeTrue();
            secondPage.HasNext.Should().BeFalse();
        }

        [Test]
        public async Task GetUserDeliveriesAsync_returns_empty_when_user_has_no_deliveries()
        {
            var userId = Random.Shared.NextInt64(1_000_000_000, long.MaxValue);

            var result = await _service.GetUserDeliveriesAsync(userId, 1, 20);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        private static DeliveryParty ValidParty(string name) => new()
        {
            Name = name,
            Contact = new DeliveryContact { Name = name, PhoneNumber = "912345678", Email = "party@example.com" },
            Address = new DeliveryAddress
            {
                AddressLine = "Rua A, 1",
                Place = "Lisboa",
                ZipCode = "1000-001",
                ZipCodePlace = "Lisboa",
                CountryCode = "PT"
            }
        };

        private static CreateDeliveryRequest ValidRequest() => new()
        {
            Details = new DeliveryDetails
            {
                ClientReference = "REF-001",
                NumberOfVolumes = 2,
                TotalWeightOfVolumes = 4.5m,
                Amount = null,
                Instructions = "Handle with care",
                PreferentialPeriod = "09:00-13:00"
            },
            Sender = ValidParty("Acme Warehouse"),
            Receiver = ValidParty("Jane Receiver"),
            Attributes = new DeliveryAttributes { Pod = true, SameDay = false, CashOnDelivery = false }
        };
    }
}
