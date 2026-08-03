namespace PackageDelivery.Infrastructure.Entities;

public partial class Event
{
    public long Id { get; set; }

    public long DeliveryId { get; set; }

    public long? PackageId { get; set; }

    public int EventTypeId { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public Delivery Delivery { get; set; } = null!;

    public Package? Package { get; set; }

    public EventType EventType { get; set; } = null!;
}
