using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class BrandConfiguration
        : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(x => x.BrandId);

            builder.Property(x => x.BrandId)
                .HasColumnName("brandId")
                .HasColumnType("char(36)");

            builder.Property(x => x.BrandName)
                .HasColumnName("brandName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(x => x.IsOnSale)
                .HasColumnName("isOnSale");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}