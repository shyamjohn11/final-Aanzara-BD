using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class BannerConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            builder.ToTable("Banners");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.Subtitle)
                .HasColumnName("subtitle")
                .HasMaxLength(150);

            builder.Property(x => x.ImageUrl)
                .HasColumnName("imageUrl")
                .HasMaxLength(1000);

            builder.Property(x => x.Link)
                .HasColumnName("link")
                .HasMaxLength(1000);

            builder.Property(x => x.Position)
                .HasColumnName("position")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnName("startDate")
                .HasMaxLength(10);

            builder.Property(x => x.EndDate)
                .HasColumnName("endDate")
                .HasMaxLength(10);

            builder.Property(x => x.Clicks)
                .HasColumnName("clicks")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}
