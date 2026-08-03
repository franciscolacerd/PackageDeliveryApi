namespace PackageDelivery.Features.Deliveries.CreateDelivery.Models
{
    public class CreateDeliveryRequest
    {
        public DeliveryDetails Details { get; set; } = null!;
        public DeliveryParty Sender { get; set; } = null!;
        public DeliveryParty Receiver { get; set; } = null!;
        public DeliveryAttributes Attributes { get; set; } = null!;
    }

    public class DeliveryDetails
    {
        public string? ClientReference { get; set; }
        public int NumberOfVolumes { get; set; }
        public decimal TotalWeightOfVolumes { get; set; }
        public decimal? Amount { get; set; }
        public string? Instructions { get; set; }
        public string? PreferentialPeriod { get; set; }
    }

    public class DeliveryParty
    {
        public string Name { get; set; } = null!;
        public DeliveryContact Contact { get; set; } = null!;
        public DeliveryAddress Address { get; set; } = null!;
    }

    public class DeliveryContact
    {
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
    }

    public class DeliveryAddress
    {
        public string AddressLine { get; set; } = null!;
        public string? Place { get; set; }
        public string ZipCode { get; set; } = null!;
        public string ZipCodePlace { get; set; } = null!;
        public string? CountryCode { get; set; }
    }

    public class DeliveryAttributes
    {
        public bool Pod { get; set; }
        public bool SameDay { get; set; }
        public bool CashOnDelivery { get; set; }
    }
}
