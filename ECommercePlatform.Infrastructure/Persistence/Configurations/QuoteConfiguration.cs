using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
        {
            builder.ToTable("Quotes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)");

            builder.Property(x => x.CustomerName)
                .HasColumnName("customerName")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320);

            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(30);

            builder.Property(x => x.Product)
                .HasColumnName("product")
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.Message)
                .HasColumnName("message")
                .HasColumnType("text");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");
        }
    }
}
