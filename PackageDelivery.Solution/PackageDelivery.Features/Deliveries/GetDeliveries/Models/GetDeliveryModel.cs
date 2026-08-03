namespace PackageDelivery.Features.Deliveries.GetDeliveries.Models
{
    public class GetDeliveryModel
    {
        public long Id { get; set; }
        public string BarCode { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public int NumberOfVolumes { get; set; }
        public decimal TotalWeightOfVolumes { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
