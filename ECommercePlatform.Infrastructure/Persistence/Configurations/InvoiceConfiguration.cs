using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(x => x.InvoiceId);

            builder.Property(x => x.InvoiceId)
                .HasColumnName("invoiceId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.InvoiceNumber)
                .HasColumnName("invoiceNumber")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.GstBreakdownJson)
                .HasColumnName("gstBreakdownJson")
                .HasColumnType("json");

            builder.Property(x => x.PdfUrl)
                .HasColumnName("pdfUrl")
                .HasMaxLength(500);

            builder.Property(x => x.GeneratedAt)
                .HasColumnName("generatedAt")
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}