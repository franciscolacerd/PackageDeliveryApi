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
            RuleFor(x => x.Sender).SetValidator(new DeliveryPartyValidator("Sender")).When(x => x.Sender != null);
            RuleFor(x => x.Receiver).SetValidator(new DeliveryPartyValidator("Receiver")).When(x => x.Receiver != null);

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
            RuleFor(x => x.ClientReference).MaximumLength(50).OverridePropertyName("Details.ClientReference");
            RuleFor(x => x.NumberOfVolumes).GreaterThan(0).LessThanOrEqualTo(200).OverridePropertyName("Details.NumberOfVolumes");
            RuleFor(x => x.TotalWeightOfVolumes).GreaterThan(0.0001m).LessThanOrEqualTo(99999).OverridePropertyName("Details.TotalWeightOfVolumes");
            RuleFor(x => x.Amount).LessThanOrEqualTo(9999999.99m).OverridePropertyName("Details.Amount");
            RuleFor(x => x.Instructions).MaximumLength(250).OverridePropertyName("Details.Instructions");
            RuleFor(x => x.PreferentialPeriod).MaximumLength(23).OverridePropertyName("Details.PreferentialPeriod");
        }
    }

    public class DeliveryPartyValidator : AbstractValidator<DeliveryParty>
    {
        public DeliveryPartyValidator(string field)
        {
            RuleFor(x => x.Name).NotNull().MaximumLength(100).OverridePropertyName($"{field}.Name");
            RuleFor(x => x.Contact).NotNull().OverridePropertyName($"{field}.Contact");
            RuleFor(x => x.Address).NotNull().OverridePropertyName($"{field}.Address");

            RuleFor(x => x.Contact).SetValidator(new DeliveryContactValidator(field)).When(x => x.Contact != null);
            RuleFor(x => x.Address).SetValidator(new DeliveryAddressValidator(field)).When(x => x.Address != null);
        }
    }

    public class DeliveryContactValidator : AbstractValidator<DeliveryContact>
    {
        public DeliveryContactValidator(string field)
        {
            RuleFor(x => x.Name).NotNull().MaximumLength(200).OverridePropertyName($"{field}.Contact.Name");
            RuleFor(x => x.PhoneNumber).NotNull().MaximumLength(100).OverridePropertyName($"{field}.Contact.PhoneNumber");
            RuleFor(x => x.Email).MaximumLength(100).EmailAddress().OverridePropertyName($"{field}.Contact.Email");
        }
    }

    public class DeliveryAddressValidator : AbstractValidator<DeliveryAddress>
    {
        public DeliveryAddressValidator(string field)
        {
            RuleFor(x => x.AddressLine).NotNull().MaximumLength(400).OverridePropertyName($"{field}.Address.AddressLine");
            RuleFor(x => x.Place).MaximumLength(100).OverridePropertyName($"{field}.Address.Place");
            RuleFor(x => x.ZipCode).NotNull().NotEmpty().MaximumLength(10).OverridePropertyName($"{field}.Address.ZipCode");
            RuleFor(x => x).Must(ValidateZipCode)
                .WithMessage("'{PropertyName}' must be valid.").OverridePropertyName($"{field}.Address.ZipCode");
            RuleFor(x => x.ZipCodePlace).NotNull().MaximumLength(100).OverridePropertyName($"{field}.Address.ZipCodePlace");
            RuleFor(x => x.CountryCode).MaximumLength(3).OverridePropertyName($"{field}.Address.CountryCode");
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
