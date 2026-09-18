using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations
{
    public sealed class DealerConfiguration : IEntityTypeConfiguration<Dealer>
    {
        public void Configure(EntityTypeBuilder<Dealer> builder)
        {
            builder.ToTable("Dealers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.DealerCode)
                .HasColumnName("dealerCode")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.DealerCode)
                .IsUnique()
                .HasDatabaseName("UX_Dealers_DealerCode");

            builder.HasIndex(x => x.AgentId)
                .HasDatabaseName("IX_Dealers_AgentId");

            builder.Property(x => x.ShopName)
                .HasColumnName("shopName")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.OwnerName)
                .HasColumnName("ownerName")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320);

            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.AlternatePhone)
                .HasColumnName("alternatePhone")
                .HasMaxLength(30);

            builder.Property(x => x.Address)
                .HasColumnName("address")
                .HasMaxLength(1000);

            builder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .HasColumnName("state")
                .HasMaxLength(100);

            builder.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(100);

            builder.Property(x => x.Pincode)
                .HasColumnName("pincode")
                .HasMaxLength(20);

            builder.Property(x => x.GSTNumber)
                .HasColumnName("gstNumber")
                .HasMaxLength(30);

            builder.Property(x => x.PANNumber)
                .HasColumnName("panNumber")
                .HasMaxLength(20);

            builder.Property(x => x.ShopDescription)
                .HasColumnName("shopDescription")
                .HasColumnType("text");

            builder.Property(x => x.ShopLogo)
                .HasColumnName("shopLogo")
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_Dealers_Status");

            builder.Property(x => x.CreatedAt).HasColumnName("createdAt");
            builder.Property(x => x.UpdatedAt).HasColumnName("updatedAt");

            // An agent's dealers are never cascade-deleted: removing an agent
            // with dealers must fail loudly instead of orphaning shop history.
            builder.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
