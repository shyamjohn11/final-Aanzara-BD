using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .HasColumnType("char(36)")
            .HasConversion(id => id.ToString(), value => Guid.Parse(value))
            .ValueGeneratedNever();

        builder.Property(u => u.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(150)
            .IsRequired();

        // A case-insensitive collation is what actually makes lookups by email
        // case-insensitive, and what makes the unique index reject Bob@x.com when
        // bob@x.com exists.
        builder.Property(u => u.Email)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        // Profile extended fields (migration AddProfileExtendedFields).
        // Lengths must stay in sync with the migration/snapshot column types
        // (nvarchar(500)/nvarchar(30)); otherwise EF reports pending model
        // changes and DatabaseInitializer.MigrateAsync throws at start-up.
        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Gender)
            .HasMaxLength(30);

        builder.Property(u => u.HashedPassphrase)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(u => u.Status).HasDatabaseName("IX_Users_Status");

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}