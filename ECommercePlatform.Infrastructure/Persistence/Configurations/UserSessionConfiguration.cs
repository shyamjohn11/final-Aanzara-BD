using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.RefreshTokenHash)
            .HasMaxLength(200)
            .IsRequired();

        // Refresh lands here on every call, so the lookup must be an index seek.
        builder.HasIndex(s => s.RefreshTokenHash)
            .IsUnique()
            .HasDatabaseName("UX_UserSessions_RefreshTokenHash");

        // Covers "active sessions for this user", the other hot path.
        builder.HasIndex(s => new { s.UserId, s.ExpiresAtUtc })
            .HasDatabaseName("IX_UserSessions_UserId_ExpiresAtUtc");

        builder.Property(s => s.RevokedReason).HasMaxLength(100);
        builder.Property(s => s.CreatedByIp).HasMaxLength(64);
        builder.Property(s => s.UserAgent).HasMaxLength(512);

        builder.Ignore(s => s.IsRevoked);
    }
}
