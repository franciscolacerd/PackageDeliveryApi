using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Services;
using PackageDelivery.Features.Tests._strapper;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Enums;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class CreateDeliveryServiceTests
    {
        private ServiceProvider _serviceProvider = null!;
        private ICreateDeliveryService _service = null!;
        private PackageDeliveryDbContext _context = null!;
        private readonly List<string> _createdBarCodes = new();

        [SetUp]
        public void Setup()
        {
            _serviceProvider = Bootstrapper.Bind();
            _service = _serviceProvider.GetRequiredService<ICreateDeliveryService>();
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
        public async Task CreateAsync_with_valid_request_persists_delivery_in_database()
        {
            var result = await _service.CreateAsync(ValidRequest(), userId: 1);

            if (result.BarCode is not null)
                _createdBarCodes.Add(result.BarCode);

            result.Success.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.BarCode.Should().MatchRegex("^[0-9]{15}$");

            var delivery = await _context.Deliveries
                .Include(d => d.Packages)
                .Include(d => d.Events)
                .Include(d => d.DeliveryDeliveryAttributes)
                .AsNoTracking()
                .SingleAsync(d => d.BarCode == result.BarCode);

            delivery.UserId.Should().Be(1);
            delivery.NumberOfVolumes.Should().Be(2);
            delivery.Packages.Should().HaveCount(2);
            delivery.Packages.Should().OnlyContain(p => p.PackageBarCode.StartsWith(result.BarCode!));
            delivery.DeliveryDeliveryAttributes.Should().ContainSingle()
                .Which.DeliveryAttributeId.Should().Be((int)DeliveryAttributeType.Pod);
            delivery.Events.Should().ContainSingle()
                .Which.EventTypeId.Should().Be((int)EventTypeCode.CreatedDelivery);
        }

        [Test]
        public async Task CreateAsync_with_invalid_request_fails_and_persists_nothing()
        {
            var request = ValidRequest();
            request.Details.NumberOfVolumes = 0;

            var result = await _service.CreateAsync(request, userId: 1);

            if (result.BarCode is not null)
                _createdBarCodes.Add(result.BarCode);

            result.Success.Should().BeFalse();
            result.BarCode.Should().BeNull();
            result.Errors.Should().NotBeEmpty();
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
