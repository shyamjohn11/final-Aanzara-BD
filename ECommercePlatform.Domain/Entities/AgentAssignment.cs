using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class AgentAssignment : AuditableEntity
    {
        public Guid AssignmentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid AgentId { get; set; }

        public Guid AssignedByAdminId { get; set; }

        public AgentAssignmentStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;

        public Agent Agent { get; set; } = null!;

        public User AssignedByAdmin { get; set; } = null!;
    }
}