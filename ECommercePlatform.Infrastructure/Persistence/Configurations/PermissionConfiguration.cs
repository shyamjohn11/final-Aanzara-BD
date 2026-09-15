using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class PermissionConfiguration
        : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(x => x.PermissionId);

            builder.Property(x => x.PermissionId)
                .HasColumnName("permissionId")
                .HasColumnType("char(36)");

            builder.Property(x => x.PermissionName)
                .HasColumnName("permissionName")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Module)
                .HasColumnName("module")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}