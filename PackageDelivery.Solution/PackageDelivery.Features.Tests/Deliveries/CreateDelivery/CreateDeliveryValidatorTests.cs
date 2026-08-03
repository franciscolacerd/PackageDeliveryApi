using AwesomeAssertions;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class CreateDeliveryValidatorTests
    {
        private readonly CreateDeliveryValidator _validator = new();

        private static DeliveryParty ValidParty(string name) => new()
        {
            Name = name,
            Contact = new DeliveryContact
            {
                Name = name,
                PhoneNumber = "912345678",
                Email = "party@example.com"
            },
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

        [Test]
        public void Valid_request_passes()
        {
            _validator.Validate(ValidRequest()).IsValid.Should().BeTrue();
        }

        [Test]
        public void Details_is_required()
        {
            var model = ValidRequest();
            model.Details = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details");
        }

        [Test]
        public void Sender_is_required()
        {
            var model = ValidRequest();
            model.Sender = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender");
        }

        [Test]
        public void Receiver_is_required()
        {
            var model = ValidRequest();
            model.Receiver = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Receiver");
        }

        [Test]
        public void Attributes_is_required()
        {
            var model = ValidRequest();
            model.Attributes = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Attributes");
        }

        [Test]
        public void Amount_without_cashOnDelivery_fails()
        {
            var model = ValidRequest();
            model.Details.Amount = 100m;
            model.Attributes.CashOnDelivery = false;

            var result = _validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("CashOnDelivery attribute must be selected"));
        }

        [Test]
        public void CashOnDelivery_without_amount_fails()
        {
            var model = ValidRequest();
            model.Details.Amount = null;
            model.Attributes.CashOnDelivery = true;

            var result = _validator.Validate(model);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("amount must be set"));
        }

        [Test]
        public void Amount_with_cashOnDelivery_passes()
        {
            var model = ValidRequest();
            model.Details.Amount = 100m;
            model.Attributes.CashOnDelivery = true;

            _validator.Validate(model).IsValid.Should().BeTrue();
        }
    }
}
