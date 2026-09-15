using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class RefundConfiguration
        : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(x => x.RefundId);

            builder.Property(x => x.RefundId)
                .HasColumnName("refundId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.PaymentId)
                .HasColumnName("paymentId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.GatewayRefundId)
                .HasColumnName("gatewayRefundId")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Payment)
                .WithMany()
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}