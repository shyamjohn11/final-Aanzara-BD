using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class SavedPaymentMethodConfiguration
        : IEntityTypeConfiguration<SavedPaymentMethod>
    {
        public void Configure(
            EntityTypeBuilder<SavedPaymentMethod> builder)
        {
            builder.ToTable("SavedPaymentMethods");

            builder.HasKey(x => x.SavedMethodId);

            builder.Property(x => x.SavedMethodId)
                .HasColumnName("savedMethodId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.PaymentMethod)
                .HasColumnName("paymentMethod")
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.GatewayToken)
                .HasColumnName("gatewayToken")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.DisplayLabel)
                .HasColumnName("displayLabel")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.IsDefault)
                .HasColumnName("isDefault")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}