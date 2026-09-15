using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class StoreOfferConfiguration : IEntityTypeConfiguration<StoreOffer>
    {
        public void Configure(EntityTypeBuilder<StoreOffer> builder)
        {
            builder.ToTable("StoreOffers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.StoreId)
                .HasColumnName("storeId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.StoreName)
                .HasColumnName("storeName")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(x => x.DiscountValue)
                .HasColumnName("discountValue")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.StartsAt)
                .HasColumnName("startsAt");

            builder.Property(x => x.EndsAt)
                .HasColumnName("endsAt");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}
