using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class StoreImageConfiguration
        : IEntityTypeConfiguration<StoreImage>
    {
        public void Configure(EntityTypeBuilder<StoreImage> builder)
        {
            builder.ToTable("StoreImages");

            // Primary Key
            builder.HasKey(x => x.ImageId);

            builder.Property(x => x.ImageId)
                .HasColumnName("imageId")
                .HasColumnType("char(36)");

            // Foreign Key
            builder.Property(x => x.StoreId)
                .HasColumnName("storeId")
                .HasColumnType("char(36)")
                .IsRequired();

            // Image URL
            builder.Property(x => x.ImageUrl)
                .HasColumnName("imageUrl")
                .HasMaxLength(500)
                .IsRequired();

            // Is Primary
            builder.Property(x => x.IsPrimary)
                .HasColumnName("isPrimary")
                .IsRequired();

            // Display Order
            builder.Property(x => x.DisplayOrder)
                .HasColumnName("displayOrder")
                .IsRequired();

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // Store → StoreImages
            builder.HasOne(x => x.Store)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}