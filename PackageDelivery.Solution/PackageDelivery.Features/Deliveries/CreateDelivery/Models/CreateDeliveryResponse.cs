using PackageDelivery.Shared.Models;

namespace PackageDelivery.Features.Deliveries.CreateDelivery.Models
{
    /// <summary>Result of a create-delivery request.</summary>
    public class CreateDeliveryResponse : OperationResponse
    {
        /// <summary>15-digit tracking barcode of the created delivery. Null when creation failed.</summary>
        /// <example>481920371650283</example>
        public string? BarCode { get; set; }
    }
}