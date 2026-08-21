namespace PackageDelivery.Shared.Models
{
    public abstract class OperationResponse : IOperationResponse
    {
        /// <summary>True when the operation succeeded; false when it failed.</summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>Human-readable outcome message.</summary>
        /// <example>Delivery created.</example>
        public string? Message { get; set; }

        /// <summary>Flat list of error messages. Empty when <see cref="Success"/> is true.</summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>Validation failures keyed by field. Empty when <see cref="Success"/> is true.</summary>
        public IDictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();
    }
}