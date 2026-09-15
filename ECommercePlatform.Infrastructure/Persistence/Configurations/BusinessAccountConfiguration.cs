using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class BusinessAccountConfiguration
        : IEntityTypeConfiguration<BusinessAccount>
    {
        public void Configure(EntityTypeBuilder<BusinessAccount> builder)
        {
            builder.ToTable("BusinessAccounts");

            builder.HasKey(x => x.BusinessAccountId);

            builder.Property(x => x.BusinessAccountId)
                .HasColumnName("businessAccountId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.CompanyName)
                .HasColumnName("companyName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.ShopName)
                .HasColumnName("shopName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.OwnerName)
                .HasColumnName("ownerName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.GstNumber)
                .HasColumnName("gstNumber")
                .HasMaxLength(30);

            builder.Property(x => x.DocumentUrl)
                .HasColumnName("documentUrl")
                .HasMaxLength(500);

            builder.Property(x => x.VerificationStatus)
                .HasColumnName("verificationStatus")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreditLimit)
                .HasColumnName("creditLimit")
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.CurrentBalance)
                .HasColumnName("currentBalance")
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<BusinessAccount>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}