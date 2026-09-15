using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            builder.HasKey(x => x.ShipmentId);

            builder.Property(x => x.ShipmentId)
                .HasColumnName("shipmentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.CourierName)
                .HasColumnName("courierName")
                .HasMaxLength(100);

            builder.Property(x => x.TrackingNumber)
                .HasColumnName("trackingNumber")
                .HasMaxLength(150);

            builder.Property(x => x.ShippedAt)
                .HasColumnName("shippedAt");

            builder.Property(x => x.EstimatedDeliveryDate)
                .HasColumnName("estimatedDeliveryDate")
                .HasColumnType("date");

            builder.Property(x => x.DeliveredAt)
                .HasColumnName("deliveredAt");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}