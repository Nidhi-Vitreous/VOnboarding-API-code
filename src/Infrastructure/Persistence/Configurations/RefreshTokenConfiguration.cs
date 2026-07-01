using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id");

        builder.Property(t => t.Token)
            .HasColumnName("token")
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.IsRevoked)
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
