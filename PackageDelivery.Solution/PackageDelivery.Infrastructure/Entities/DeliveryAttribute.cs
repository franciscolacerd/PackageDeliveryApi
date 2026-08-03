namespace PackageDelivery.Infrastructure.Entities;

public partial class DeliveryAttribute
{
    public int Id { get; set; }

    public string DeliveryAttributeENG { get; set; } = null!;

    public string DeliveryAttributeES { get; set; } = null!;

    public string DeliveryAttributePT { get; set; } = null!;

    public ICollection<DeliveryDeliveryAttribute> DeliveryDeliveryAttributes { get; set; } = new List<DeliveryDeliveryAttribute>();
}
