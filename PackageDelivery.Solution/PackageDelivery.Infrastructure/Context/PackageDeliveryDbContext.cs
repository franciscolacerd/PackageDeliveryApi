using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Infrastructure.Context;

public partial class PackageDeliveryDbContext : IdentityDbContext<
    AspNetUser,
    AspNetRole,
    long,
    AspNetUserClaim,
    AspNetUserRole,
    AspNetUserLogin,
    AspNetRoleClaim,
    AspNetUserToken>
{
    public PackageDeliveryDbContext(DbContextOptions<PackageDeliveryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<ApiRestLog> ApiRestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId1, "IX_AspNetRoleClaims_RoleId1");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaimRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.RoleId1Navigation).WithMany(p => p.AspNetRoleClaimRoleId1Navigations).HasForeignKey(d => d.RoleId1);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId1, "IX_AspNetUserClaims_UserId1");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaimUsers).HasForeignKey(d => d.UserId);

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.AspNetUserClaimUserId1Navigations).HasForeignKey(d => d.UserId1);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasIndex(e => e.UserId1, "IX_AspNetUserLogins_UserId1");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLoginUsers).HasForeignKey(d => d.UserId);

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.AspNetUserLoginUserId1Navigations).HasForeignKey(d => d.UserId1);
        });

        modelBuilder.Entity<AspNetUserRole>(entity =>
        {
            entity.HasIndex(e => e.RoleId1, "IX_AspNetUserRoles_RoleId1");

            entity.HasIndex(e => e.UserId1, "IX_AspNetUserRoles_UserId1");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetUserRoleRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.RoleId1Navigation).WithMany(p => p.AspNetUserRoleRoleId1Navigations).HasForeignKey(d => d.RoleId1);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserRoleUsers).HasForeignKey(d => d.UserId);

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.AspNetUserRoleUserId1Navigations).HasForeignKey(d => d.UserId1);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasIndex(e => e.UserId1, "IX_AspNetUserTokens_UserId1");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokenUsers).HasForeignKey(d => d.UserId);

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.AspNetUserTokenUserId1Navigations).HasForeignKey(d => d.UserId1);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("Deliveries");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.BarCode, "IX_Deliveries_BarCode").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_Deliveries_UserId");

            entity.Property(e => e.BarCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SenderName).HasMaxLength(200);
            entity.Property(e => e.ReceiverName).HasMaxLength(200);
            entity.Property(e => e.TotalWeight).HasColumnType("decimal(18,3)");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ApiRestLog>(entity =>
        {
            entity.ToTable("ApiRestLogs");

            entity.HasIndex(e => e.CreatedDateUtc, "IX_ApiRestLogs_CreatedDateUtc").IsDescending();

            entity.Property(e => e.Host).HasMaxLength(255);
            entity.Property(e => e.Origin).HasMaxLength(500);
            entity.Property(e => e.Path).HasMaxLength(500);
            entity.Property(e => e.QueryString).HasMaxLength(2000);
            entity.Property(e => e.Scheme).HasMaxLength(10);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
