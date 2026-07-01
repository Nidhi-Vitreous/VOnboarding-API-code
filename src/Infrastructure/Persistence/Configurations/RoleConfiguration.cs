using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(r => r.RoleType).HasColumnName("role_type").HasMaxLength(128).IsRequired();
        builder.Property(r => r.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(r => r.IsSystemRole).HasColumnName("is_system_role").HasDefaultValue(false);
        builder.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(r => r.DepartmentId).HasColumnName("department_id").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasOne(r => r.Department)
            .WithMany(d => d.Roles)
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
