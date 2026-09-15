using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class DeliveryAssignment : AuditableEntity
    {
        public Guid AssignmentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid DeliveryPartnerId { get; set; }

        public DeliveryAssignmentStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;

        public DeliveryPartner DeliveryPartner { get; set; } = null!;
    }
}