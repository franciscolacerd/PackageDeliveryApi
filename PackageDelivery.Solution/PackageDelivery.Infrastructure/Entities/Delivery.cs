namespace PackageDelivery.Infrastructure.Entities;

public partial class Delivery
{
    public long Id { get; set; }

    public string BarCode { get; set; } = null!;

    public long UserId { get; set; }

    public string? ClientReference { get; set; }

    public int NumberOfVolumes { get; set; }

    public decimal TotalWeightOfVolumes { get; set; }

    public decimal? Amount { get; set; }

    public string? Instructions { get; set; }

    public string? PreferentialPeriod { get; set; }

    public string SenderName { get; set; } = null!;

    public string? SenderContactName { get; set; }

    public string? SenderContactPhoneNumber { get; set; }

    public string? SenderContactEmail { get; set; }

    public string SenderAddress { get; set; } = null!;

    public string? SenderAddressPlace { get; set; }

    public string SenderAddressZipCode { get; set; } = null!;

    public string SenderAddressZipCodePlace { get; set; } = null!;

    public string? SenderAddressCountryCode { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string? ReceiverContactName { get; set; }

    public string? ReceiverContactPhoneNumber { get; set; }

    public string? ReceiverContactEmail { get; set; }

    public string ReceiverAddress { get; set; } = null!;

    public string? ReceiverAddressPlace { get; set; }

    public string ReceiverAddressZipCode { get; set; } = null!;

    public string ReceiverAddressZipCodePlace { get; set; } = null!;

    public string? ReceiverAddressCountryCode { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? UpdatedDateUtc { get; set; }

    public byte[]? Version { get; set; }

    public ICollection<Package> Packages { get; set; } = new List<Package>();

    public ICollection<DeliveryDeliveryAttribute> DeliveryDeliveryAttributes { get; set; } = new List<DeliveryDeliveryAttribute>();

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
