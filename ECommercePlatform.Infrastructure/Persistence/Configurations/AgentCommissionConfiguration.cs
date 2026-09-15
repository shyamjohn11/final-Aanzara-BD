using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AgentCommissionConfiguration
        : IEntityTypeConfiguration<AgentCommission>
    {
        public void Configure(EntityTypeBuilder<AgentCommission> builder)
        {
            builder.ToTable("AgentCommission");

            builder.HasKey(x => x.CommissionId);

            builder.Property(x => x.CommissionId)
                .HasColumnName("commissionId")
                .HasColumnType("char(36)");

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.CommissionType)
                .HasColumnName("commissionType")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Value)
                .HasColumnName("value")
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt")
                .IsRequired();

            builder.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}