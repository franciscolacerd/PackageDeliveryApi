namespace PackageDelivery.Features.Deliveries.GetDeliveries.Models
{
    /// <summary>A delivery that belongs to the authenticated user.</summary>
    public class GetDeliveryModel
    {
        /// <summary>Unique delivery identifier.</summary>
        /// <example>1024</example>
        public long Id { get; set; }

        /// <summary>15-digit tracking barcode of the delivery.</summary>
        /// <example>481920371650283</example>
        public string BarCode { get; set; } = string.Empty;

        /// <summary>Name of the sender.</summary>
        /// <example>Acme Warehouse</example>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Name of the receiver.</summary>
        /// <example>Jane Receiver</example>
        public string ReceiverName { get; set; } = string.Empty;

        /// <summary>Number of volumes (packages) in the delivery.</summary>
        /// <example>2</example>
        public int NumberOfVolumes { get; set; }

        /// <summary>Total weight of all volumes, in kilograms.</summary>
        /// <example>4.5</example>
        public decimal TotalWeightOfVolumes { get; set; }

        /// <summary>Creation timestamp, in UTC.</summary>
        public DateTime CreatedDateUtc { get; set; }
    }
}
