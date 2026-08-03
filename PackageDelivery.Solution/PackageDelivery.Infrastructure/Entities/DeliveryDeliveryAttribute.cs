namespace PackageDelivery.Infrastructure.Entities;

public partial class DeliveryDeliveryAttribute
{
    public long Id { get; set; }

    public long DeliveryId { get; set; }

    public int DeliveryAttributeId { get; set; }

    public Delivery Delivery { get; set; } = null!;

    public DeliveryAttribute DeliveryAttribute { get; set; } = null!;
}
