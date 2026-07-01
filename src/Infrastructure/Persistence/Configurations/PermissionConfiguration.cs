using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.SystemName).HasColumnName("system_name").HasMaxLength(128).IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(512).IsRequired();

        builder.HasIndex(p => p.SystemName).IsUnique();
    }
}
