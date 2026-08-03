namespace PackageDelivery.Features.Deliveries.CreateDelivery.Models
{
    /// <summary>Result of a create-delivery request.</summary>
    public class CreateDeliveryResponse
    {
        /// <summary>True when the delivery was created; false when validation failed.</summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>15-digit tracking barcode of the created delivery. Null when creation failed.</summary>
        /// <example>481920371650283</example>
        public string? BarCode { get; set; }

        /// <summary>Human-readable outcome message.</summary>
        /// <example>Delivery created.</example>
        public string? Message { get; set; }

        /// <summary>Validation error messages. Empty when <see cref="Success"/> is true.</summary>
        public List<string> Errors { get; set; } = new();
    }
}
