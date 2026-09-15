using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(x => x.RoleId);

            builder.Property(x => x.RoleId)
                .HasColumnName("roleId")
                .HasColumnType("char(36)");

            builder.Property(x => x.RoleName)
                .HasColumnName("roleName")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}