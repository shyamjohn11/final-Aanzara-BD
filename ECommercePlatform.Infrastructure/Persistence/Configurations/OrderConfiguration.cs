using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            // Primary Key
            builder.HasKey(x => x.OrderId);

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)");

            // User
            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            // Order Status
            builder.Property(x => x.OrderStatus)
                .HasColumnName("orderStatus")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Items Total
            builder.Property(x => x.ItemsTotal)
                .HasColumnName("itemsTotal")
                .HasPrecision(10, 2)
                .IsRequired();

            // Discount
            builder.Property(x => x.Discount)
                .HasColumnName("discount")
                .HasPrecision(10, 2)
                .IsRequired();

            // GST
            builder.Property(x => x.GstAmount)
                .HasColumnName("gstAmount")
                .HasPrecision(10, 2)
                .IsRequired();

            // Delivery Charge
            builder.Property(x => x.DeliveryCharge)
                .HasColumnName("deliveryCharge")
                .HasPrecision(10, 2)
                .IsRequired();

            // Handling Fee
            builder.Property(x => x.HandlingFee)
                .HasColumnName("handlingFee")
                .HasPrecision(10, 2)
                .IsRequired();

            // Grand Total
            builder.Property(x => x.GrandTotal)
                .HasColumnName("grandTotal")
                .HasPrecision(10, 2)
                .IsRequired();

            // Applied Coupon
            builder.Property(x => x.AppliedCouponId)
                .HasColumnName("appliedCouponId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // Updated At
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            // User → Orders
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Coupon → Orders
            builder.HasOne(x => x.AppliedCoupon)
                .WithMany()
                .HasForeignKey(x => x.AppliedCouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order → OrderItems
            builder.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}