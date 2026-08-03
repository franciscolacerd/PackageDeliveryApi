using Microsoft.AspNetCore.Identity;

namespace PackageDelivery.Infrastructure.Entities;

public partial class AspNetRoleClaim : IdentityRoleClaim<long>
{
    public long? RoleId1 { get; set; }

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual AspNetRole? RoleId1Navigation { get; set; }
}
