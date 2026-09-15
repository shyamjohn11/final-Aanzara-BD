using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class OrderAddressConfiguration
        : IEntityTypeConfiguration<OrderAddress>
    {
        public void Configure(EntityTypeBuilder<OrderAddress> builder)
        {
            builder.ToTable("OrderAddresses");

            builder.HasKey(x => x.OrderAddressId);

            builder.Property(x => x.OrderAddressId)
                .HasColumnName("orderAddressId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.HasIndex(x => x.OrderId)
                .IsUnique();

            builder.Property(x => x.RecipientName)
                .HasColumnName("recipientName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.RecipientPhone)
                .HasColumnName("recipientPhone")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.AddressLine1)
                .HasColumnName("addressLine1")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AddressLine2)
                .HasColumnName("addressLine2")
                .HasMaxLength(255);

            builder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.State)
                .HasColumnName("state")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Pincode)
                .HasColumnName("pincode")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 7)
                .IsRequired();

            builder.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(10, 7)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithOne(x => x.ShippingAddress)
                .HasForeignKey<OrderAddress>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}