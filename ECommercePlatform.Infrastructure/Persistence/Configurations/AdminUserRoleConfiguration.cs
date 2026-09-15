using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AdminUserRoleConfiguration
        : IEntityTypeConfiguration<AdminUserRole>
    {
        public void Configure(EntityTypeBuilder<AdminUserRole> builder)
        {
            builder.ToTable("AdminUserRoles");

            builder.HasKey(x => new
            {
                x.AdminUserId,
                x.RoleId
            });

            builder.Property(x => x.AdminUserId)
                .HasColumnName("adminUserId")
                .HasColumnType("char(36)");

            builder.Property(x => x.RoleId)
                .HasColumnName("roleId")
                .HasColumnType("char(36)");

            builder.HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}