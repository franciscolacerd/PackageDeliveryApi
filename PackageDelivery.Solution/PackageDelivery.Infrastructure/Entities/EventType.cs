namespace PackageDelivery.Infrastructure.Entities;

public partial class EventType
{
    public int Id { get; set; }

    public string EventTypeENG { get; set; } = null!;

    public string EventTypeES { get; set; } = null!;

    public string EventTypePT { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
