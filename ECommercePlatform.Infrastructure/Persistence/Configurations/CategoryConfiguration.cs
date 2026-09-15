using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value))
            .ValueGeneratedNever();

        builder.Property(c => c.CategoryCode).HasMaxLength(50).IsRequired();
        builder.Property(c => c.CategoryName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);

        builder.HasIndex(c => c.CategoryCode)
            .IsUnique()
            .HasDatabaseName("UX_Categories_CategoryCode");

        builder.HasIndex(c => c.IsActive).HasDatabaseName("IX_Categories_IsActive");

        builder.HasMany(c => c.SubCategories)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            // Restrict, not Cascade: deleting a category must not silently take a
            // subtree of products with it. The handler refuses instead.
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Images)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            // Images cannot outlive their category, so these do cascade.
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
