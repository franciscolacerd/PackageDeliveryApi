using AwesomeAssertions;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class DeliveryAddressValidatorTests
    {
        private readonly DeliveryAddressValidator _validator = new("Sender");

        private static DeliveryAddress ValidAddress() => new()
        {
            AddressLine = "Rua A, 1",
            Place = "Lisboa",
            ZipCode = "1000-001",
            ZipCodePlace = "Lisboa",
            CountryCode = "PT"
        };

        [Test]
        public void Valid_address_passes()
        {
            _validator.Validate(ValidAddress()).IsValid.Should().BeTrue();
        }

        [Test]
        public void AddressLine_is_required()
        {
            var model = ValidAddress();
            model.AddressLine = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.AddressLine");
        }

        [Test]
        public void AddressLine_over_400_chars_fails()
        {
            var model = ValidAddress();
            model.AddressLine = new string('x', 401);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.AddressLine");
        }

        [Test]
        public void Place_over_100_chars_fails()
        {
            var model = ValidAddress();
            model.Place = new string('x', 101);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.Place");
        }

        [Test]
        public void ZipCode_is_required()
        {
            var model = ValidAddress();
            model.ZipCode = string.Empty;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.ZipCode");
        }

        [Test]
        public void ZipCode_over_10_chars_fails()
        {
            var model = ValidAddress();
            model.CountryCode = "ES";
            model.ZipCode = new string('1', 11);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.ZipCode");
        }

        [TestCase("1234")]
        [TestCase("abcd-efg")]
        [TestCase("0000-000")]
        public void ZipCode_with_invalid_portuguese_format_fails(string zipCode)
        {
            var model = ValidAddress();
            model.CountryCode = "PT";
            model.ZipCode = zipCode;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.ZipCode");
        }

        [Test]
        public void ZipCode_for_non_portuguese_country_skips_format_check()
        {
            var model = ValidAddress();
            model.CountryCode = "ES";
            model.ZipCode = "28001";

            _validator.Validate(model).Errors
                .Should().NotContain(e => e.PropertyName == "Sender.Address.ZipCode");
        }

        [Test]
        public void ZipCodePlace_is_required()
        {
            var model = ValidAddress();
            model.ZipCodePlace = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.ZipCodePlace");
        }

        [Test]
        public void ZipCodePlace_over_100_chars_fails()
        {
            var model = ValidAddress();
            model.ZipCodePlace = new string('x', 101);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.ZipCodePlace");
        }

        [Test]
        public void CountryCode_over_3_chars_fails()
        {
            var model = ValidAddress();
            model.CountryCode = "PRTT";

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Sender.Address.CountryCode");
        }
    }
}
