using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Configurations;

public sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.MerchantName).HasColumnName("merchant_name").HasMaxLength(256).IsRequired();
        builder.Property(m => m.LegalName).HasColumnName("legal_name").HasMaxLength(256);
        builder.Property(m => m.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(m => m.Phone).HasColumnName("phone").HasMaxLength(32);
        builder.Property(m => m.Address).HasColumnName("address").HasMaxLength(512);
        builder.Property(m => m.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(m => m.Product).HasColumnName("product").HasMaxLength(128);
        builder.Property(m => m.Status).HasColumnName("status").HasMaxLength(64).IsRequired();
        builder.Property(m => m.Role).HasColumnName("role").HasMaxLength(128).IsRequired();
        builder.Property(m => m.CreatedBy).HasColumnName("created_by");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.Role);
        builder.HasIndex(m => m.Product);
        builder.HasIndex(m => m.MerchantName);
    }
}

public sealed class MerchantStatusHistoryConfiguration : IEntityTypeConfiguration<MerchantStatusHistory>
{
    public void Configure(EntityTypeBuilder<MerchantStatusHistory> builder)
    {
        builder.ToTable("merchant_status_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.MerchantId).HasColumnName("merchant_id");
        builder.Property(h => h.OldStatus).HasColumnName("old_status").HasMaxLength(64);
        builder.Property(h => h.NewStatus).HasColumnName("new_status").HasMaxLength(64).IsRequired();
        builder.Property(h => h.ChangedBy).HasColumnName("changed_by");
        builder.Property(h => h.ChangedAt).HasColumnName("changed_at");
        builder.Property(h => h.Comment).HasColumnName("comment").HasMaxLength(1000);

        builder.HasOne(h => h.Merchant)
            .WithMany(m => m.StatusHistory)
            .HasForeignKey(h => h.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.MerchantId);
    }
}

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.EntityType).HasColumnName("entity_type").HasMaxLength(64).IsRequired();
        builder.Property(a => a.EntityId).HasColumnName("entity_id");
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(64).IsRequired();
        builder.Property(a => a.OldValueJson).HasColumnName("old_value").HasColumnType("jsonb");
        builder.Property(a => a.NewValueJson).HasColumnName("new_value").HasColumnType("jsonb");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UserId).HasColumnName("user_id");

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
