using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");

            // Primary Key
            builder.HasKey(x => x.StoreId);

            builder.Property(x => x.StoreId)
                .HasColumnName("storeId")
                .HasColumnType("char(36)");

            // Store Name
            builder.Property(x => x.StoreName)
                .HasColumnName("storeName")
                .HasMaxLength(150)
                .IsRequired();

            // Address
            builder.Property(x => x.Address)
                .HasColumnName("address")
                .HasMaxLength(255);

            // Latitude
            builder.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 7);

            // Longitude
            builder.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(10, 7);

            // Contact Number
            builder.Property(x => x.ContactNumber)
                .HasColumnName("contactNumber")
                .HasMaxLength(20);

            // Opening Hours
            builder.Property(x => x.OpeningHours)
                .HasColumnName("openingHours")
                .HasMaxLength(255);

            // Status
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Created At
            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            // Updated At
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}