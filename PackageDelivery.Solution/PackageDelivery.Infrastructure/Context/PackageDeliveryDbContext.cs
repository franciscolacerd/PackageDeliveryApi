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

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<DeliveryAttribute> DeliveryAttributes { get; set; }

    public virtual DbSet<DeliveryDeliveryAttribute> DeliveryDeliveryAttributes { get; set; }

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
            entity.Property(e => e.ClientReference).HasMaxLength(50);
            entity.Property(e => e.TotalWeightOfVolumes).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Instructions).HasMaxLength(250);
            entity.Property(e => e.PreferentialPeriod).HasMaxLength(23);

            entity.Property(e => e.SenderName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SenderContactName).HasMaxLength(200);
            entity.Property(e => e.SenderContactPhoneNumber).HasMaxLength(100);
            entity.Property(e => e.SenderContactEmail).HasMaxLength(100);
            entity.Property(e => e.SenderAddress).HasMaxLength(400).IsRequired();
            entity.Property(e => e.SenderAddressPlace).HasMaxLength(100);
            entity.Property(e => e.SenderAddressZipCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.SenderAddressZipCodePlace).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SenderAddressCountryCode).HasMaxLength(3);

            entity.Property(e => e.ReceiverName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ReceiverContactName).HasMaxLength(200);
            entity.Property(e => e.ReceiverContactPhoneNumber).HasMaxLength(100);
            entity.Property(e => e.ReceiverContactEmail).HasMaxLength(100);
            entity.Property(e => e.ReceiverAddress).HasMaxLength(400).IsRequired();
            entity.Property(e => e.ReceiverAddressPlace).HasMaxLength(100);
            entity.Property(e => e.ReceiverAddressZipCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ReceiverAddressZipCodePlace).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ReceiverAddressCountryCode).HasMaxLength(3);

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasMany(e => e.Packages)
                .WithOne(p => p.Delivery)
                .HasForeignKey(p => p.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("Packages");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.DeliveryId, "IX_Packages_DeliveryId");

            entity.HasIndex(e => e.PackageBarCode, "IX_Packages_PackageBarCode");

            entity.Property(e => e.PackageBarCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Weight).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventTypes");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventTypeENG).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventTypeES).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventTypePT).HasMaxLength(100).IsRequired();

            entity.HasData(
                new EventType { Id = 1, EventTypeENG = "Created delivery", EventTypeES = "Envío creado", EventTypePT = "Entrega criada" },
                new EventType { Id = 2, EventTypeENG = "Picked up by carrier", EventTypeES = "Recogido por el transportista", EventTypePT = "Recolhido pelo transportador" },
                new EventType { Id = 3, EventTypeENG = "In transit", EventTypeES = "En tránsito", EventTypePT = "Em trânsito" },
                new EventType { Id = 4, EventTypeENG = "Delivered to receiver", EventTypeES = "Entregado al destinatario", EventTypePT = "Entregue ao destinatário" },
                new EventType { Id = 5, EventTypeENG = "Unable to make pickup", EventTypeES = "No se pudo recoger", EventTypePT = "Não foi possível recolher" },
                new EventType { Id = 6, EventTypeENG = "Unable to make delivery", EventTypeES = "No se pudo entregar", EventTypePT = "Não foi possível entregar" },
                new EventType { Id = 7, EventTypeENG = "Returned to sender", EventTypeES = "Devuelto al remitente", EventTypePT = "Devolvido ao remetente" });
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.DeliveryId, "IX_Events_DeliveryId");
            entity.HasIndex(e => e.PackageId, "IX_Events_PackageId");
            entity.HasIndex(e => e.EventTypeId, "IX_Events_EventTypeId");

            entity.HasOne(e => e.Delivery).WithMany(d => d.Events)
                .HasForeignKey(e => e.DeliveryId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Package).WithMany()
                .HasForeignKey(e => e.PackageId).OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.EventType).WithMany(t => t.Events)
                .HasForeignKey(e => e.EventTypeId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DeliveryAttribute>(entity =>
        {
            entity.ToTable("DeliveryAttributes");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DeliveryAttributeENG).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeliveryAttributeES).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeliveryAttributePT).HasMaxLength(100).IsRequired();

            entity.HasData(
                new DeliveryAttribute { Id = 1, DeliveryAttributeENG = "Proof of delivery", DeliveryAttributeES = "Prueba de entrega", DeliveryAttributePT = "Prova de entrega" },
                new DeliveryAttribute { Id = 2, DeliveryAttributeENG = "Same day", DeliveryAttributeES = "Mismo día", DeliveryAttributePT = "Mesmo dia" },
                new DeliveryAttribute { Id = 3, DeliveryAttributeENG = "Cash on delivery", DeliveryAttributeES = "Contra reembolso", DeliveryAttributePT = "Pagamento na entrega" });
        });

        modelBuilder.Entity<DeliveryDeliveryAttribute>(entity =>
        {
            entity.ToTable("DeliveryDeliveryAttributes");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.DeliveryId, e.DeliveryAttributeId }, "IX_DeliveryDeliveryAttributes_Delivery_Attribute").IsUnique();

            entity.HasOne(e => e.Delivery).WithMany(d => d.DeliveryDeliveryAttributes)
                .HasForeignKey(e => e.DeliveryId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DeliveryAttribute).WithMany(a => a.DeliveryDeliveryAttributes)
                .HasForeignKey(e => e.DeliveryAttributeId).OnDelete(DeleteBehavior.NoAction);
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
