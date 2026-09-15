using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class AgentCommissionEarningConfiguration
        : IEntityTypeConfiguration<AgentCommissionEarning>
    {
        public void Configure(
            EntityTypeBuilder<AgentCommissionEarning> builder)
        {
            builder.ToTable("AgentCommissionEarnings");

            builder.HasKey(x => x.EarningId);

            builder.Property(x => x.EarningId)
                .HasColumnName("earningId")
                .HasColumnType("char(36)");

            builder.Property(x => x.AgentId)
                .HasColumnName("agentId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.CommissionId)
                .HasColumnName("commissionId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.AmountEarned)
                .HasColumnName("amountEarned")
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

            builder.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Commission)
                .WithMany()
                .HasForeignKey(x => x.CommissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}