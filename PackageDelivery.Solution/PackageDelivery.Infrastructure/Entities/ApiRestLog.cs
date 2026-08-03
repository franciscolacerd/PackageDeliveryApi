namespace PackageDelivery.Infrastructure.Entities;

public partial class ApiRestLog
{
    public long Id { get; set; }

    public string Scheme { get; set; } = null!;

    public string Host { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string QueryString { get; set; } = null!;

    public string RequestBody { get; set; } = null!;

    public string ResponseBody { get; set; } = null!;

    public DateTime? CreatedDateUtc { get; set; }

    public string Origin { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public byte[] Version { get; set; } = null!;
}
