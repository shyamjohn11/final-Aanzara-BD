using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class PaymentConfiguration
        : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.PaymentId);

            builder.Property(x => x.PaymentId)
                .HasColumnName("paymentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.PaymentMethod)
                .HasColumnName("paymentMethod")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.PaymentGateway)
                .HasColumnName("paymentGateway")
                .HasMaxLength(50);

            builder.Property(x => x.GatewayTransactionId)
                .HasColumnName("gatewayTransactionId")
                .HasMaxLength(150);

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasMaxLength(10)
                .HasDefaultValue("INR")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.FailureReason)
                .HasColumnName("failureReason")
                .HasMaxLength(255);

            builder.Property(x => x.PaidAt)
                .HasColumnName("paidAt");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}