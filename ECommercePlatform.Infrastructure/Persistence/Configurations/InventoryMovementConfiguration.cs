using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class InventoryMovementConfiguration
        : IEntityTypeConfiguration<InventoryMovement>
    {
        public void Configure(EntityTypeBuilder<InventoryMovement> builder)
        {
            builder.ToTable("InventoryMovements");

            builder.HasKey(x => x.MovementId);

            builder.Property(x => x.MovementId)
                .HasColumnName("movementId")
                .HasColumnType("char(36)");

            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.WarehouseId)
                .HasColumnName("warehouseId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasMaxLength(255);

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("createdByUserId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // Product
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Warehouse
            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - nullable
            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // User - nullable
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}