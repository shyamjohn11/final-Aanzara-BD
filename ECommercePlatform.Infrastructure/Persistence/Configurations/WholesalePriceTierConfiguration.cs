using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class WholesalePriceTierConfiguration
        : IEntityTypeConfiguration<WholesalePriceTier>
    {
        public void Configure(
            EntityTypeBuilder<WholesalePriceTier> builder)
        {
            builder.ToTable("WholesalePriceTiers");

            builder.HasKey(x => x.TierId);

            builder.Property(x => x.TierId)
                .HasColumnName("tierId")
                .HasColumnType("char(36)");

            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.MinQuantity)
                .HasColumnName("minQuantity")
                .IsRequired();

            builder.Property(x => x.MaxQuantity)
                .HasColumnName("maxQuantity")
                .IsRequired(false);

            builder.Property(x => x.PricePerUnit)
                .HasColumnName("pricePerUnit")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Product → WholesalePriceTiers
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}