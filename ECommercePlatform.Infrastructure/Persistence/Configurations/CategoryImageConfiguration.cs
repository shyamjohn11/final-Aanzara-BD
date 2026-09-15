using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class CategoryImageConfiguration : IEntityTypeConfiguration<CategoryImage>
{
    public void Configure(EntityTypeBuilder<CategoryImage> builder)
    {
        builder.ToTable("CategoryImages");

        builder.HasKey(i => i.CategoryImageId);

        builder.Property(i => i.CategoryImageId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value))
            .ValueGeneratedNever();

        builder.Property(i => i.CategoryId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value));

        builder.Property(i => i.ImageUrl).HasMaxLength(2000).IsRequired();
        builder.Property(i => i.FileName).HasMaxLength(260).IsRequired();
        builder.Property(i => i.FilePath).HasMaxLength(1000).IsRequired();
        builder.Property(i => i.ContentType).HasMaxLength(100).IsRequired();

        builder.HasIndex(i => new { i.CategoryId, i.DisplayOrder })
            .HasDatabaseName("IX_CategoryImages_CategoryId_DisplayOrder");

        // A filtered unique index makes "at most one primary per category" a
        // database guarantee rather than a convention handlers must remember.
        builder.HasIndex(i => i.CategoryId)
            .IsUnique()
            .HasFilter("[IsPrimary] = 1")
            .HasDatabaseName("UX_CategoryImages_CategoryId_Primary");
    }
}
