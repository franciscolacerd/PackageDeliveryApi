using AwesomeAssertions;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Features.Deliveries.CreateDelivery.Validators;

namespace PackageDelivery.Features.Tests.Deliveries.CreateDelivery
{
    public class DeliveryContactValidatorTests
    {
        private readonly DeliveryContactValidator _validator = new();

        private static DeliveryContact ValidContact() => new()
        {
            Name = "John Sender",
            PhoneNumber = "912345678",
            Email = "john@example.com"
        };

        [Test]
        public void Valid_contact_passes()
        {
            _validator.Validate(ValidContact()).IsValid.Should().BeTrue();
        }

        [Test]
        public void Name_is_required()
        {
            var model = ValidContact();
            model.Name = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Name");
        }

        [Test]
        public void Name_over_200_chars_fails()
        {
            var model = ValidContact();
            model.Name = new string('x', 201);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Name");
        }

        [Test]
        public void PhoneNumber_is_required()
        {
            var model = ValidContact();
            model.PhoneNumber = null!;

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "PhoneNumber");
        }

        [Test]
        public void PhoneNumber_over_100_chars_fails()
        {
            var model = ValidContact();
            model.PhoneNumber = new string('9', 101);

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "PhoneNumber");
        }

        [Test]
        public void Email_can_be_null()
        {
            var model = ValidContact();
            model.Email = null;

            _validator.Validate(model).Errors
                .Should().NotContain(e => e.PropertyName == "Email");
        }

        [Test]
        public void Email_with_invalid_format_fails()
        {
            var model = ValidContact();
            model.Email = "not-an-email";

            _validator.Validate(model).Errors
                .Should().Contain(e => e.PropertyName == "Email");
        }
    }
}
