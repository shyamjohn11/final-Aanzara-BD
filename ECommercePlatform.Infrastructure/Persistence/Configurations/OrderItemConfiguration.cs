using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class OrderItemConfiguration
        : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            // Primary Key
            builder.HasKey(x => x.OrderItemId);

            builder.Property(x => x.OrderItemId)
                .HasColumnName("orderItemId")
                .HasColumnType("char(36)");

            // Order Id
            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            // Product Id
            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            // Quantity
            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            // Unit Price
            builder.Property(x => x.UnitPrice)
                .HasColumnName("unitPrice")
                .HasPrecision(10, 2)
                .IsRequired();

            // Order → OrderItems
            builder.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → OrderItems
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}