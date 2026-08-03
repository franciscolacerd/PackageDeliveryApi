namespace PackageDelivery.Features.Deliveries.CreateDelivery.Models
{
    public class CreateDeliveryResponse
    {
        public bool Success { get; set; }
        public string? BarCode { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
