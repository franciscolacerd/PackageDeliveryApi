using System.Text;
using PackageDelivery.Features.Deliveries.CreateDelivery.Models;
using PackageDelivery.Infrastructure.Entities;
using PackageDelivery.Infrastructure.Enums;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Builders
{
    public class DeliveryBuilder
    {
        private const int BarCodeLength = 15;

        private readonly Delivery _delivery = new();

        public DeliveryBuilder ForUser(long userId)
        {
            _delivery.UserId = userId;
            return this;
        }

        public DeliveryBuilder WithDetails(DeliveryDetails details)
        {
            _delivery.ClientReference = details.ClientReference;
            _delivery.NumberOfVolumes = details.NumberOfVolumes;
            _delivery.TotalWeightOfVolumes = details.TotalWeightOfVolumes;
            _delivery.Amount = details.Amount;
            _delivery.Instructions = details.Instructions;
            _delivery.PreferentialPeriod = details.PreferentialPeriod;
            return this;
        }

        public DeliveryBuilder WithSender(DeliveryParty sender)
        {
            _delivery.SenderName = sender.Name;
            _delivery.SenderContactName = sender.Contact?.Name;
            _delivery.SenderContactPhoneNumber = sender.Contact?.PhoneNumber;
            _delivery.SenderContactEmail = sender.Contact?.Email;
            _delivery.SenderAddress = sender.Address.AddressLine;
            _delivery.SenderAddressPlace = sender.Address.Place;
            _delivery.SenderAddressZipCode = sender.Address.ZipCode;
            _delivery.SenderAddressZipCodePlace = sender.Address.ZipCodePlace;
            _delivery.SenderAddressCountryCode = sender.Address.CountryCode;
            return this;
        }

        public DeliveryBuilder WithReceiver(DeliveryParty receiver)
        {
            _delivery.ReceiverName = receiver.Name;
            _delivery.ReceiverContactName = receiver.Contact?.Name;
            _delivery.ReceiverContactPhoneNumber = receiver.Contact?.PhoneNumber;
            _delivery.ReceiverContactEmail = receiver.Contact?.Email;
            _delivery.ReceiverAddress = receiver.Address.AddressLine;
            _delivery.ReceiverAddressPlace = receiver.Address.Place;
            _delivery.ReceiverAddressZipCode = receiver.Address.ZipCode;
            _delivery.ReceiverAddressZipCodePlace = receiver.Address.ZipCodePlace;
            _delivery.ReceiverAddressCountryCode = receiver.Address.CountryCode;
            return this;
        }

        public DeliveryBuilder WithAttributes(DeliveryAttributes attributes)
        {
            AddAttribute(attributes.Pod, DeliveryAttributeType.Pod);
            AddAttribute(attributes.SameDay, DeliveryAttributeType.SameDay);
            AddAttribute(attributes.CashOnDelivery, DeliveryAttributeType.CashOnDelivery);
            return this;
        }

        public Delivery Build()
        {
            var now = DateTime.UtcNow;

            _delivery.Status = "Created";
            _delivery.BarCode = GenerateBarCode();
            _delivery.CreatedDate = DateTime.Now;
            _delivery.CreatedDateUtc = now;

            AddPackages(now);
            AddCreatedEvent(now);

            return _delivery;
        }

        private void AddAttribute(bool selected, DeliveryAttributeType type)
        {
            if (!selected)
                return;

            _delivery.DeliveryDeliveryAttributes.Add(new DeliveryDeliveryAttribute
            {
                DeliveryAttributeId = (int)type
            });
        }

        private void AddCreatedEvent(DateTime createdDateUtc)
        {
            _delivery.Events.Add(new Event
            {
                EventTypeId = (int)EventTypeCode.CreatedDelivery,
                CreatedDateUtc = createdDateUtc
            });
        }

        private void AddPackages(DateTime createdDateUtc)
        {
            var weightPerVolume = _delivery.NumberOfVolumes > 0
                ? _delivery.TotalWeightOfVolumes / _delivery.NumberOfVolumes
                : _delivery.TotalWeightOfVolumes;

            for (var i = 1; i <= _delivery.NumberOfVolumes; i++)
            {
                _delivery.Packages.Add(new Package
                {
                    PackageNumber = i,
                    Weight = weightPerVolume,
                    PackageBarCode = $"{_delivery.BarCode}{i.ToString().PadLeft(3, '0')}",
                    CreatedDateUtc = createdDateUtc
                });
            }
        }

        private static string GenerateBarCode()
        {
            var builder = new StringBuilder(BarCodeLength);

            for (var i = 0; i < BarCodeLength; i++)
                builder.Append(Random.Shared.Next(0, 10));

            return builder.ToString();
        }
    }
}
