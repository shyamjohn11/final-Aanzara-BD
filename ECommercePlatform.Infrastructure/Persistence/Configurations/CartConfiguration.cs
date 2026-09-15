using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(x => x.CartId);

            builder.Property(x => x.CartId)
                .HasColumnName("cartId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            // UNIQUE userId
            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.AppliedCouponId)
                .HasColumnName("appliedCouponId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            // User → Cart
            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Cart>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Coupon → Cart
            builder.HasOne(x => x.AppliedCoupon)
                .WithMany()
                .HasForeignKey(x => x.AppliedCouponId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}