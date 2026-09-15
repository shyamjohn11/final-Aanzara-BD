using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class SavedItemConfiguration
        : IEntityTypeConfiguration<SavedItem>
    {
        public void Configure(EntityTypeBuilder<SavedItem> builder)
        {
            builder.ToTable("SavedItems");

            builder.HasKey(x => x.SavedItemId);

            builder.Property(x => x.SavedItemId)
                .HasColumnName("savedItemId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // User → SavedItems
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → SavedItems
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}