using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class ShopLedgerConfiguration
        : IEntityTypeConfiguration<ShopLedger>
    {
        public void Configure(EntityTypeBuilder<ShopLedger> builder)
        {
            builder.ToTable("ShopLedger");

            builder.HasKey(x => x.LedgerId);

            builder.Property(x => x.LedgerId)
                .HasColumnName("ledgerId")
                .HasColumnType("char(36)");

            builder.Property(x => x.BusinessAccountId)
                .HasColumnName("businessAccountId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)");

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasColumnName("note")
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.HasOne(x => x.BusinessAccount)
                .WithMany()
                .HasForeignKey(x => x.BusinessAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}