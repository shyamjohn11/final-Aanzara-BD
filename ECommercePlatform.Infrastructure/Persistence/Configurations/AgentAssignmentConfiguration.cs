using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AgentAssignmentConfiguration
        : IEntityTypeConfiguration<AgentAssignment>
    {
        public void Configure(EntityTypeBuilder<AgentAssignment> builder)
        {
            builder.ToTable("AgentAssignments");

            builder.HasKey(x => x.AssignmentId);

            builder.Property(x => x.AssignmentId)
                .HasColumnName("assignmentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
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

            builder.Property(x => x.CompletedAt)
                .HasColumnName("completedAt");

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.AssignedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}