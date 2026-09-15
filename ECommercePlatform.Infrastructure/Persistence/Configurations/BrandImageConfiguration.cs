using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class BrandImageConfiguration
        : IEntityTypeConfiguration<BrandImage>
    {
        public void Configure(EntityTypeBuilder<BrandImage> builder)
        {
            builder.ToTable("BrandImages");

            builder.HasKey(x => x.BrandImageId);

            builder.Property(x => x.BrandImageId)
                .HasColumnName("brandImageId")
                .HasColumnType("char(36)");

            builder.Property(x => x.BrandId)
                .HasColumnName("brandId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.ImageUrl)
                .HasColumnName("imageUrl")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.IsPrimary)
                .HasColumnName("isPrimary");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.HasOne(x => x.Brand)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}