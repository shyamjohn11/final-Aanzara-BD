using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class DeliveryRuleConfiguration
        : IEntityTypeConfiguration<DeliveryRule>
    {
        public void Configure(EntityTypeBuilder<DeliveryRule> builder)
        {
            builder.ToTable("DeliveryRules");

            // Primary Key
            builder.HasKey(x => x.DeliveryRuleId);

            builder.Property(x => x.DeliveryRuleId)
                .HasColumnName("deliveryRuleId")
                .HasColumnType("char(36)");

            // Minimum Order Value for Free Delivery
            builder.Property(x => x.MinOrderValueForFreeDelivery)
                .HasColumnName("minOrderValueForFreeDelivery")
                .HasPrecision(10, 2)
                .IsRequired();

            // Flat Delivery Charge
            builder.Property(x => x.FlatDeliveryCharge)
                .HasColumnName("flatDeliveryCharge")
                .HasPrecision(10, 2)
                .IsRequired();

            // Handling Fee
            builder.Property(x => x.HandlingFee)
                .HasColumnName("handlingFee")
                .HasPrecision(10, 2)
                .IsRequired();
        }
    }
}