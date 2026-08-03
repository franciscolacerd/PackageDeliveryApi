using Microsoft.AspNetCore.Identity;

namespace PackageDelivery.Infrastructure.Entities;

public partial class AspNetUserClaim : IdentityUserClaim<long>
{
    public long? UserId1 { get; set; }

    public virtual AspNetUser User { get; set; } = null!;

    public virtual AspNetUser? UserId1Navigation { get; set; }
}
