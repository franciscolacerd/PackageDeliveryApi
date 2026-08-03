using Microsoft.AspNetCore.Identity;

namespace PackageDelivery.Infrastructure.Entities;

public partial class AspNetRole : IdentityRole<long>
{
    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaimRoleId1Navigations { get; set; } = new List<AspNetRoleClaim>();

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaimRoles { get; set; } = new List<AspNetRoleClaim>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoleRoleId1Navigations { get; set; } = new List<AspNetUserRole>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoleRoles { get; set; } = new List<AspNetUserRole>();
}
