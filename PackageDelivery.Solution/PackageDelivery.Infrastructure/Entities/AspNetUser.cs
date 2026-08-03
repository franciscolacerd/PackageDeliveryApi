using Microsoft.AspNetCore.Identity;

namespace PackageDelivery.Infrastructure.Entities;

public partial class AspNetUser : IdentityUser<long>
{
    public string? CreatedBy { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDateUtc { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte[]? Version { get; set; }

    public bool Active { get; set; }

    public string? Name { get; set; }

    public string? FullName { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaimUserId1Navigations { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserClaim> AspNetUserClaimUsers { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLoginUserId1Navigations { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLoginUsers { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoleUserId1Navigations { get; set; } = new List<AspNetUserRole>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoleUsers { get; set; } = new List<AspNetUserRole>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokenUserId1Navigations { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokenUsers { get; set; } = new List<AspNetUserToken>();
}
