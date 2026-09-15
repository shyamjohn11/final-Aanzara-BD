using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class DeliveryZoneConfiguration
        : IEntityTypeConfiguration<DeliveryZone>
    {
        public void Configure(EntityTypeBuilder<DeliveryZone> builder)
        {
            builder.ToTable("DeliveryZones");

            builder.HasKey(x => x.ZoneId);

            builder.Property(x => x.ZoneId)
                .HasColumnName("zoneId")
                .HasColumnType("char(36)");

            builder.Property(x => x.ZoneName)
                .HasColumnName("zoneName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Pincodes)
                .HasColumnName("pincodes")
                .HasColumnType("text");

            builder.Property(x => x.RadiusKm)
                .HasColumnName("radiusKm")
                .IsRequired();
        }
    }
}