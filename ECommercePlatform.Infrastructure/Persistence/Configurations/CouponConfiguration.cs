using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");

            // Primary Key
            builder.HasKey(x => x.CouponId);

            builder.Property(x => x.CouponId)
                .HasColumnName("couponId")
                .HasColumnType("char(36)");

            // Coupon Code
            builder.Property(x => x.CouponCode)
                .HasColumnName("couponCode")
                .HasMaxLength(30)
                .IsRequired();

            // UNIQUE couponCode
            builder.HasIndex(x => x.CouponCode)
                .IsUnique();

            // Discount Type
            builder.Property(x => x.DiscountType)
                .HasColumnName("discountType")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Discount Value
            builder.Property(x => x.DiscountValue)
                .HasColumnName("discountValue")
                .HasPrecision(10, 2)
                .IsRequired();

            // Minimum Order Value
            builder.Property(x => x.MinOrderValue)
                .HasColumnName("minOrderValue")
                .HasPrecision(10, 2)
                .IsRequired();

            // Usage Limit Per User
            builder.Property(x => x.UsageLimitPerUser)
                .HasColumnName("usageLimitPerUser")
                .IsRequired();

            // Start Date
            builder.Property(x => x.StartDate)
                .HasColumnName("startDate")
                .IsRequired();

            // End Date
            builder.Property(x => x.EndDate)
                .HasColumnName("endDate")
                .IsRequired();

            // Status
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}