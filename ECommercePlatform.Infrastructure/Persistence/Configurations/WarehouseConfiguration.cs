using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(x => x.WarehouseId);

            builder.Property(x => x.WarehouseId)
                .HasColumnName("warehouseId")
                .HasColumnType("char(36)");

            builder.Property(x => x.WarehouseName)
                .HasColumnName("warehouseName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Address)
                .HasColumnName("address")
                .HasMaxLength(255);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}