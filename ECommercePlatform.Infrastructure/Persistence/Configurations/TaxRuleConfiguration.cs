using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
    {
        public void Configure(EntityTypeBuilder<TaxRule> builder)
        {
            builder.ToTable("TaxRules");

            // Primary Key
            builder.HasKey(x => x.TaxRuleId);

            builder.Property(x => x.TaxRuleId)
                .HasColumnName("taxRuleId")
                .HasColumnType("char(36)");

            // CategoryId - Nullable FK
            builder.Property(x => x.CategoryId)
                .HasColumnName("categoryId")
                .HasColumnType("char(36)")
                .IsRequired(false);

            // GST Percentage
            builder.Property(x => x.GstPercentage)
                .HasColumnName("gstPercentage")
                .HasPrecision(5, 2)
                .IsRequired();

            // Category → TaxRules
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}