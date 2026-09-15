using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
{
    public void Configure(EntityTypeBuilder<SubCategory> builder)
    {
        builder.ToTable("SubCategories");

        builder.HasKey(s => s.SubCategoryId);

        builder.Property(s => s.SubCategoryId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value))
            .ValueGeneratedNever();

        builder.Property(s => s.CategoryId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value));

        builder.Property(s => s.SubCategoryCode).HasMaxLength(50).IsRequired();
        builder.Property(s => s.SubCategoryName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);

        builder.HasIndex(s => s.SubCategoryCode)
            .IsUnique()
            .HasDatabaseName("UX_SubCategories_SubCategoryCode");

        builder.HasIndex(s => new { s.CategoryId, s.IsActive })
            .HasDatabaseName("IX_SubCategories_CategoryId_IsActive");

        builder.HasMany(s => s.Products)
            .WithOne(p => p.SubCategory)
            .HasForeignKey(p => p.SubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
