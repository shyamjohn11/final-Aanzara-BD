using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class OrderStatusHistoryConfiguration
        : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistory");

            builder.HasKey(x => x.HistoryId);

            builder.Property(x => x.HistoryId)
                .HasColumnName("historyId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ChangedByUserId)
                .HasColumnName("changedByUserId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(255);

            builder.Property(x => x.ChangedAt)
                .HasColumnName("changedAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}