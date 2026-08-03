namespace PackageDelivery.Infrastructure.Entities;

public partial class Delivery
{
    public long Id { get; set; }

    public string BarCode { get; set; } = null!;

    public long UserId { get; set; }

    public string? SenderName { get; set; }

    public string? ReceiverName { get; set; }

    public string Status { get; set; } = "Created";

    public int NumberOfVolumes { get; set; }

    public decimal TotalWeight { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? UpdatedDateUtc { get; set; }

    public byte[]? Version { get; set; }
}
