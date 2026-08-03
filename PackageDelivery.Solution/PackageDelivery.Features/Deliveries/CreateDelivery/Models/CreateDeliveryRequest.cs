namespace PackageDelivery.Features.Deliveries.CreateDelivery.Models
{
    /// <summary>Payload to create a new delivery.</summary>
    public class CreateDeliveryRequest
    {
        /// <summary>Delivery details (volumes, weight, references).</summary>
        public DeliveryDetails Details { get; set; } = null!;

        /// <summary>Sender party (name, contact and address).</summary>
        public DeliveryParty Sender { get; set; } = null!;

        /// <summary>Receiver party (name, contact and address).</summary>
        public DeliveryParty Receiver { get; set; } = null!;

        /// <summary>Optional delivery attributes.</summary>
        public DeliveryAttributes Attributes { get; set; } = null!;
    }

    /// <summary>Delivery details.</summary>
    public class DeliveryDetails
    {
        /// <summary>Free-text client reference. Max 50 characters.</summary>
        /// <example>REF-001</example>
        public string? ClientReference { get; set; }

        /// <summary>Number of volumes (packages). Between 1 and 200.</summary>
        /// <example>2</example>
        public int NumberOfVolumes { get; set; }

        /// <summary>Total weight of all volumes, in kilograms. Greater than 0 and up to 99999.</summary>
        /// <example>4.5</example>
        public decimal TotalWeightOfVolumes { get; set; }

        /// <summary>Cash-on-delivery amount. Required when the CashOnDelivery attribute is selected.</summary>
        /// <example>19.90</example>
        public decimal? Amount { get; set; }

        /// <summary>Delivery instructions. Max 250 characters.</summary>
        /// <example>Leave at the front desk</example>
        public string? Instructions { get; set; }

        /// <summary>Preferential delivery period. Max 23 characters.</summary>
        /// <example>09:00-13:00</example>
        public string? PreferentialPeriod { get; set; }
    }

    /// <summary>A delivery party (sender or receiver).</summary>
    public class DeliveryParty
    {
        /// <summary>Party name. Max 100 characters.</summary>
        /// <example>Acme Warehouse</example>
        public string Name { get; set; } = null!;

        /// <summary>Contact details.</summary>
        public DeliveryContact Contact { get; set; } = null!;

        /// <summary>Address.</summary>
        public DeliveryAddress Address { get; set; } = null!;
    }

    /// <summary>Contact details of a party.</summary>
    public class DeliveryContact
    {
        /// <summary>Contact name. Max 200 characters.</summary>
        /// <example>John Sender</example>
        public string Name { get; set; } = null!;

        /// <summary>Phone number. Max 100 characters.</summary>
        /// <example>912345678</example>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>Email address. Optional; max 100 characters.</summary>
        /// <example>sender@example.com</example>
        public string? Email { get; set; }
    }

    /// <summary>Address of a party.</summary>
    public class DeliveryAddress
    {
        /// <summary>Street address. Max 400 characters.</summary>
        /// <example>Rua A, 1</example>
        public string AddressLine { get; set; } = null!;

        /// <summary>Place / locality. Max 100 characters.</summary>
        /// <example>Lisboa</example>
        public string? Place { get; set; }

        /// <summary>Postal code. For Portugal it must match the NNNN-NNN format.</summary>
        /// <example>1000-001</example>
        public string ZipCode { get; set; } = null!;

        /// <summary>Postal code place. Max 100 characters.</summary>
        /// <example>Lisboa</example>
        public string ZipCodePlace { get; set; } = null!;

        /// <summary>ISO country code. Max 3 characters; defaults to Portugal when empty.</summary>
        /// <example>PT</example>
        public string? CountryCode { get; set; }
    }

    /// <summary>Optional delivery attributes.</summary>
    public class DeliveryAttributes
    {
        /// <summary>Proof of delivery required.</summary>
        /// <example>true</example>
        public bool Pod { get; set; }

        /// <summary>Same-day delivery.</summary>
        /// <example>false</example>
        public bool SameDay { get; set; }

        /// <summary>Cash on delivery. Requires <c>Details.Amount</c> to be set.</summary>
        /// <example>false</example>
        public bool CashOnDelivery { get; set; }
    }
}
