using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private const string MoneyType = "decimal(10,2)";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductId)
            .HasColumnName("productId")
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value))
            .ValueGeneratedNever();

        builder.Property(p => p.CategoryId)
            .HasColumnName("categoryId")
            .HasColumnType("char(36)")
            .HasConversion(
                id => id == null ? null : id.ToString(),
                value => value == null ? null : Guid.Parse(value));

        builder.Property(p => p.SubCategoryId)
            .HasColumnName("subCategoryId")
            .HasColumnType("char(36)")
            .HasConversion(
                id => id == null ? null : id.ToString(),
                value => value == null ? null : Guid.Parse(value));

        builder.Property(p => p.BrandId)
            .HasColumnName("brandId")
            .HasColumnType("char(36)")
            .HasConversion(
                id => id == null ? null : id.ToString(),
                value => value == null ? null : Guid.Parse(value));

        builder.Property(p => p.DealerId)
            .HasColumnName("dealerId")
            .HasColumnType("char(36)")
            .HasConversion(
                id => id == null ? null : id.ToString(),
                value => value == null ? null : Guid.Parse(value));

        builder.Property(p => p.ProductName)
            .HasColumnName("productName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(p => p.Specification)
            .HasColumnName("specification")
            .HasColumnType("text");

        builder.Property(p => p.Price).HasColumnName("price").HasColumnType(MoneyType);
        builder.Property(p => p.Mrp).HasColumnName("mrp").HasColumnType(MoneyType);
        builder.Property(p => p.Discount).HasColumnName("discount").HasColumnType("decimal(5,2)");
        builder.Property(p => p.Moq).HasColumnName("moq");
        builder.Property(p => p.IsOrganic).HasColumnName("isOrganic");
        builder.Property(p => p.IsGstFree).HasColumnName("isGstFree");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CreatedAt).HasColumnName("createdAt");
        builder.Property(p => p.UpdatedAt).HasColumnName("updatedAt");

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("UX_Products_Sku");

        builder.HasIndex(p => p.SubCategoryId).HasDatabaseName("IX_Products_SubCategoryId");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("IX_Products_CategoryId");
        builder.HasIndex(p => p.ProductName).HasDatabaseName("IX_Products_ProductName");
        builder.HasIndex(p => p.DealerId).HasDatabaseName("IX_Products_DealerId");

        // Dealer products are never cascade-deleted: a dealer with products
        // must be emptied (or deactivated) before it can be removed.
        builder.HasOne(p => p.Dealer)
            .WithMany()
            .HasForeignKey(p => p.DealerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}