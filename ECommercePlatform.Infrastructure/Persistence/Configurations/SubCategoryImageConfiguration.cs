using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class SubCategoryImageConfiguration
        : IEntityTypeConfiguration<SubCategoryImage>
    {
        public void Configure(EntityTypeBuilder<SubCategoryImage> builder)
        {
            builder.ToTable("SubCategoryImages");

            // Primary Key
            builder.HasKey(x => x.ImageId);

            builder.Property(x => x.ImageId)
                .HasColumnName("imageId")
                .HasColumnType("char(36)");

            // SubCategory FK
            builder.Property(x => x.SubCategoryId)
                .HasColumnName("subCategoryId")
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

            // SubCategory → SubCategoryImages
            builder.HasOne(x => x.SubCategory)
                .WithMany()
                .HasForeignKey(x => x.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}