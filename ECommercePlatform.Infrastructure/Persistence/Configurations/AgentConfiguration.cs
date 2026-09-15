using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AgentConfiguration : IEntityTypeConfiguration<Agent>
    {
        public void Configure(EntityTypeBuilder<Agent> builder)
        {
            builder.ToTable("Agents");

            builder.HasKey(x => x.AgentId);

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.UserId)
                .HasColumnName("userId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.CreatedByAdminId)
                .HasColumnName("createdByAdminId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.AssignedStoreId)
                .HasColumnName("assignedStoreId")
                .HasColumnType("char(36)");

            builder.Property(x => x.EmployeeCode)
                .HasColumnName("employeeCode")
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Agent>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // assignedStoreId intentionally has no FK for now
        }
    }
}