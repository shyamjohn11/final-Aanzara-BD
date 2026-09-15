using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class ShopAssignmentConfiguration
        : IEntityTypeConfiguration<ShopAssignment>
    {
        public void Configure(EntityTypeBuilder<ShopAssignment> builder)
        {
            builder.ToTable("ShopAssignments");

            builder.HasKey(x => x.AssignmentId);

            builder.Property(x => x.AssignmentId)
                .HasColumnName("assignmentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.BusinessAccountId)
                .HasColumnName("businessAccountId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.AssignedByAdminId)
                .HasColumnName("assignedByAdminId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.AssignedAt)
                .HasColumnName("assignedAt")
                .IsRequired();

            // UNIQUE(agentId, businessAccountId)
            builder.HasIndex(x => new
            {
                x.AgentId,
                x.BusinessAccountId
            })
            .IsUnique();

            builder.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BusinessAccount)
                .WithMany()
                .HasForeignKey(x => x.BusinessAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.AssignedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}