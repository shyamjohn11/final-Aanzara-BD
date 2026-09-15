using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class DeliveryPartnerConfiguration
        : IEntityTypeConfiguration<DeliveryPartner>
    {
        public void Configure(EntityTypeBuilder<DeliveryPartner> builder)
        {
            builder.ToTable("DeliveryPartners");

            builder.HasKey(x => x.DeliveryPartnerId);

            builder.Property(x => x.DeliveryPartnerId)
                .HasColumnName("deliveryPartnerId")
                .HasColumnType("char(36)");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            builder.Property(x => x.VehicleType)
                .HasColumnName("vehicleType")
                .HasMaxLength(50);

            builder.Property(x => x.ZoneId)
                .HasColumnName("zoneId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasOne(x => x.Zone)
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}