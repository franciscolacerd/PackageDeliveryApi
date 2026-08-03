namespace PackageDelivery.Features.Deliveries.GetDeliveries.Models
{
    public class GetDeliveryModel
    {
        public long Id { get; set; }
        public string BarCode { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumberOfVolumes { get; set; }
        public decimal TotalWeight { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
