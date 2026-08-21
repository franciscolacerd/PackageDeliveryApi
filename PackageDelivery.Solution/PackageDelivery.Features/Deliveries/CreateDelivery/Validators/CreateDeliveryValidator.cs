using System.Text.RegularExpressions;
using FluentValidation;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Validators
{
    public class CreateDeliveryValidator : AbstractValidator<CreateDeliveryRequest>
    {
        public CreateDeliveryValidator()
        {
            RuleFor(x => x.Details).NotNull();
            RuleFor(x => x.Sender).NotNull();
            RuleFor(x => x.Receiver).NotNull();
            RuleFor(x => x.Attributes).NotNull();

            RuleFor(x => x.Details).SetValidator(new DeliveryDetailsValidator()).When(x => x.Details != null);
            RuleFor(x => x.Sender).SetValidator(new DeliveryPartyValidator()).When(x => x.Sender != null);
            RuleFor(x => x.Receiver).SetValidator(new DeliveryPartyValidator()).When(x => x.Receiver != null);

            RuleFor(x => x)
                .Must(x => !(x.Details?.Amount > 0) || x.Attributes?.CashOnDelivery == true)
                .WithMessage("If amount is set, the CashOnDelivery attribute must be selected.");

            RuleFor(x => x)
                .Must(x => x.Attributes?.CashOnDelivery != true || x.Details?.Amount > 0)
                .WithMessage("If the CashOnDelivery attribute is selected, amount must be set.");
        }
    }

    public class DeliveryDetailsValidator : AbstractValidator<DeliveryDetails>
    {
        public DeliveryDetailsValidator()
        {
            RuleFor(x => x.ClientReference).MaximumLength(50);
            RuleFor(x => x.NumberOfVolumes).GreaterThan(0).LessThanOrEqualTo(200);
            RuleFor(x => x.TotalWeightOfVolumes).GreaterThan(0.0001m).LessThanOrEqualTo(99999);
            RuleFor(x => x.Amount).LessThanOrEqualTo(9999999.99m);
            RuleFor(x => x.Instructions).MaximumLength(250);
            RuleFor(x => x.PreferentialPeriod).MaximumLength(23);
        }
    }

    public class DeliveryPartyValidator : AbstractValidator<DeliveryParty>
    {
        public DeliveryPartyValidator()
        {
            RuleFor(x => x.Name).NotNull().MaximumLength(100);
            RuleFor(x => x.Contact).NotNull();
            RuleFor(x => x.Address).NotNull();

            RuleFor(x => x.Contact).SetValidator(new DeliveryContactValidator()).When(x => x.Contact != null);
            RuleFor(x => x.Address).SetValidator(new DeliveryAddressValidator()).When(x => x.Address != null);
        }
    }

    public class DeliveryContactValidator : AbstractValidator<DeliveryContact>
    {
        public DeliveryContactValidator()
        {
            RuleFor(x => x.Name).NotNull().MaximumLength(200);
            RuleFor(x => x.PhoneNumber).NotNull().MaximumLength(100);
            RuleFor(x => x.Email).MaximumLength(100).EmailAddress();
        }
    }

    public class DeliveryAddressValidator : AbstractValidator<DeliveryAddress>
    {
        public DeliveryAddressValidator()
        {
            RuleFor(x => x.AddressLine).NotNull().MaximumLength(400);
            RuleFor(x => x.Place).MaximumLength(100);
            RuleFor(x => x.ZipCode).NotNull().NotEmpty().MaximumLength(10);
            RuleFor(x => x).Must(ValidateZipCode)
                .WithMessage("'{PropertyName}' must be valid.").OverridePropertyName("ZipCode");
            RuleFor(x => x.ZipCodePlace).NotNull().MaximumLength(100);
            RuleFor(x => x.CountryCode).MaximumLength(3);
        }

        private static bool ValidateZipCode(DeliveryAddress address)
        {
            if (address is null || string.IsNullOrEmpty(address.ZipCode))
                return false;

            var isPortugal = string.IsNullOrEmpty(address.CountryCode)
                || address.CountryCode.ToLower() == "pt"
                || address.CountryCode.ToLower() == "prt";

            return !isPortugal || Regex.IsMatch(address.ZipCode, "^[1-9]\\d{3}-\\d{3}$");
        }
    }
}
