namespace PackageDelivery.Infrastructure.Enums
{
    public enum EventTypeCode
    {
        CreatedDelivery = 1,
        PickedupByCarrier = 2,
        InTransit = 3,
        DeliveredToReceiver = 4,
        UnableToMakePickup = 5,
        UnableToMakeDelivery = 6,
        ReturnedToSender = 7
    }
}
