using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventory");

            builder.HasKey(x => x.InventoryId);

            builder.Property(x => x.InventoryId)
                .HasColumnName("inventoryId")
                .HasColumnType("char(36)");

            builder.Property(x => x.ProductId)
                .HasColumnName("productId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.WarehouseId)
                .HasColumnName("warehouseId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.StockQuantity)
                .HasColumnName("stockQuantity")
                .IsRequired();

            builder.Property(x => x.ReservedQuantity)
                .HasColumnName("reservedQuantity")
                .IsRequired();

            builder.Property(x => x.ReorderLevel)
                .HasColumnName("reorderLevel")
                .IsRequired();

            builder.Property(x => x.DispatchEstimateDays)
                .HasColumnName("dispatchEstimateDays")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            // UNIQUE(productId, warehouseId)
            builder.HasIndex(x => new
            {
                x.ProductId,
                x.WarehouseId
            })
            .IsUnique();

            // Product → Inventory
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Warehouse → Inventory
            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}