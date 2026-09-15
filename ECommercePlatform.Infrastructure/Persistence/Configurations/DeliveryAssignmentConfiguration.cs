using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommercePlatform.Infrastructure.Configurations
{
    public class DeliveryAssignmentConfiguration
        : IEntityTypeConfiguration<DeliveryAssignment>
    {
        public void Configure(
            EntityTypeBuilder<DeliveryAssignment> builder)
        {
            builder.ToTable("DeliveryAssignments");

            builder.HasKey(x => x.AssignmentId);

            builder.Property(x => x.AssignmentId)
                .HasColumnName("assignmentId")
                .HasColumnType("char(36)");

            builder.Property(x => x.OrderId)
                .HasColumnName("orderId")
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.DeliveryPartnerId)
                .HasColumnName("deliveryPartnerId")
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

            // Order → DeliveryAssignments
            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // DeliveryPartner → DeliveryAssignments
            builder.HasOne(x => x.DeliveryPartner)
                .WithMany()
                .HasForeignKey(x => x.DeliveryPartnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}