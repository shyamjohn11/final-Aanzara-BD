using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class WishlistItemConfiguration
        : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("WishlistItems");

            builder.HasKey(x => x.WishlistItemId);

            builder.Property(x => x.WishlistItemId)
                .HasColumnName("wishlistItemId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // Composite UNIQUE(userId, productId)
            builder.HasIndex(x => new
            {
                x.UserId,
                x.ProductId
            })
            .IsUnique();

            // User → WishlistItems
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → WishlistItems
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}