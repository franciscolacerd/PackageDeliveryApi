using AwesomeAssertions;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class DeliveryDetailsValidatorTests
    {
        private readonly DeliveryDetailsValidator _validator = new();

        private static DeliveryDetails ValidDetails() => new()
        {
            ClientReference = "REF-001",
            NumberOfVolumes = 2,
            TotalWeightOfVolumes = 4.5m,
            Amount = null,
            Instructions = "Handle with care",
            PreferentialPeriod = "09:00-13:00"
        };

        [Test]
        public void Valid_details_pass()
        {
            _validator.Validate(ValidDetails()).IsValid.Should().BeTrue();
        }

        [Test]
        public void ClientReference_over_50_chars_fails()
        {
            var model = ValidDetails();
            model.ClientReference = new string('x', 51);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.ClientReference");
        }

        [Test]
        public void ClientReference_at_50_chars_passes()
        {
            var model = ValidDetails();
            model.ClientReference = new string('x', 50);

            _validator.Validate(model).Errors
                .Should().NotContain(e => e.PropertyName == "Details.ClientReference");
        }

        [TestCase(0)]
        [TestCase(201)]
        public void NumberOfVolumes_out_of_range_fails(int volumes)
        {
            var model = ValidDetails();
            model.NumberOfVolumes = volumes;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.NumberOfVolumes");
        }

        [TestCase(1)]
        [TestCase(200)]
        public void NumberOfVolumes_within_range_passes(int volumes)
        {
            var model = ValidDetails();
            model.NumberOfVolumes = volumes;

            _validator.Validate(model).Errors
                .Should().NotContain(e => e.PropertyName == "Details.NumberOfVolumes");
        }

        [TestCase(0)]
        [TestCase(100000)]
        public void TotalWeightOfVolumes_out_of_range_fails(double weight)
        {
            var model = ValidDetails();
            model.TotalWeightOfVolumes = (decimal)weight;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.TotalWeightOfVolumes");
        }

        [Test]
        public void Amount_over_max_fails()
        {
            var model = ValidDetails();
            model.Amount = 10000000m;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.Amount");
        }

        [Test]
        public void Instructions_over_250_chars_fails()
        {
            var model = ValidDetails();
            model.Instructions = new string('x', 251);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.Instructions");
        }

        [Test]
        public void PreferentialPeriod_over_23_chars_fails()
        {
            var model = ValidDetails();
            model.PreferentialPeriod = new string('x', 24);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Details.PreferentialPeriod");
        }
    }
}
