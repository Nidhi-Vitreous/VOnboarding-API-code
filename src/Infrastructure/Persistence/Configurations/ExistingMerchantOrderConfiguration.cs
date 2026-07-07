using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Configurations;

public sealed class ExistingMerchantOrderConfiguration : IEntityTypeConfiguration<ExistingMerchantOrder>
{
    public void Configure(EntityTypeBuilder<ExistingMerchantOrder> builder)
    {
        builder.ToTable("existing_merchant_orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id).HasColumnName("id");
        builder.Property(order => order.MerchantName).HasColumnName("merchant_name").HasMaxLength(256).IsRequired();
        builder.Property(order => order.Product).HasColumnName("product").HasMaxLength(128);
        builder.Property(order => order.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(order => order.Status).HasColumnName("status").HasMaxLength(64).IsRequired();
        builder.Property(order => order.CreatedBy).HasColumnName("created_by");
        builder.Property(order => order.CreatedAt).HasColumnName("created_at");
        builder.Property(order => order.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(order => order.MerchantName);
        builder.HasIndex(order => order.Status);
    }
}
