namespace PackageDelivery.Infrastructure.Entities;

public partial class Package
{
    public long Id { get; set; }

    public long DeliveryId { get; set; }

    public string PackageBarCode { get; set; } = null!;

    public int PackageNumber { get; set; }

    public decimal Weight { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public byte[]? Version { get; set; }

    public Delivery Delivery { get; set; } = null!;
}
