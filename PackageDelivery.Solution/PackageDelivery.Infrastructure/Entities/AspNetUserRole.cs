using Microsoft.AspNetCore.Identity;

namespace PackageDelivery.Infrastructure.Entities;

public partial class AspNetUserRole : IdentityUserRole<long>
{
    public long? RoleId1 { get; set; }

    public long? UserId1 { get; set; }

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual AspNetRole? RoleId1Navigation { get; set; }

    public virtual AspNetUser User { get; set; } = null!;

    public virtual AspNetUser? UserId1Navigation { get; set; }
}
