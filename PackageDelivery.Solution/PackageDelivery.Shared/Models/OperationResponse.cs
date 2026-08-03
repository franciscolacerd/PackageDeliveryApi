namespace PackageDelivery.Shared.Models
{
    public class OperationResponse : IOperationResponse
    {
        public bool Success { get; private set; }
        public List<string> Errors { get; private set; } = new();
        public string? Message { get; private set; }

        private OperationResponse() { }

        public static OperationResponse WasSuccess(string? message = null) =>
            new() { Success = true, Errors = new List<string>(), Message = message };

        public static OperationResponse WasFailure(string error) =>
            new() { Success = false, Errors = new List<string> { error }, Message = null };

        public static OperationResponse WasFailure(List<string> errors) =>
            new() { Success = false, Errors = errors, Message = null };
    }
}
