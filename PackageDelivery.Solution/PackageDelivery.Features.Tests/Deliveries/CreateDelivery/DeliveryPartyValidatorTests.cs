using AwesomeAssertions;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class DeliveryPartyValidatorTests
    {
        private readonly DeliveryPartyValidator _validator = new("Sender");

        private static DeliveryParty ValidParty() => new()
        {
            Name = "Acme Warehouse",
            Contact = new DeliveryContact
            {
                Name = "John Sender",
                PhoneNumber = "912345678",
                Email = "john@example.com"
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

        [Test]
        public void Valid_party_passes()
        {
            _validator.Validate(ValidParty()).IsValid.Should().BeTrue();
        }

        [Test]
        public void Name_is_required()
        {
            var model = ValidParty();
            model.Name = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Name");
        }

        [Test]
        public void Name_over_100_chars_fails()
        {
            var model = ValidParty();
            model.Name = new string('x', 101);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Name");
        }

        [Test]
        public void Contact_is_required()
        {
            var model = ValidParty();
            model.Contact = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Contact");
        }

        [Test]
        public void Address_is_required()
        {
            var model = ValidParty();
            model.Address = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address");
        }

        [Test]
        public void Invalid_nested_contact_fails_party_validation()
        {
            var model = ValidParty();
            model.Contact.Name = null!;

            _validator.Validate(model).IsValid.Should().BeFalse();
        }
    }
}
