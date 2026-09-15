using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AddressConfiguration
        : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(x => x.AddressId);

            builder.Property(x => x.AddressId)
                .HasColumnName("addressId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Label)
                .HasColumnName("label")
                .HasMaxLength(50);

            builder.Property(x => x.AddressLine1)
                .HasColumnName("addressLine1")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AddressLine2)
                .HasColumnName("addressLine2")
                .HasMaxLength(255);

            builder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.State)
                .HasColumnName("state")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Pincode)
                .HasColumnName("pincode")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 7);

            builder.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(10, 7);

            builder.Property(x => x.IsDefault)
                .HasColumnName("isDefault")
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}