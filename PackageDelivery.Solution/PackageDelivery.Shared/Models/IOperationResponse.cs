namespace PackageDelivery.Shared.Models
{
    public interface IOperationResponse
    {
        bool Success { get; }
        List<string> Errors { get; }
    }
}
