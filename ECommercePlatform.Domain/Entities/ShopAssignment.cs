using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class ShopAssignment : AuditableEntity
    {
        public Guid AssignmentId { get; set; }

        public Guid AgentId { get; set; }

        public Guid BusinessAccountId { get; set; }

        public Guid AssignedByAdminId { get; set; }

        public ShopAssignmentStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }

        // Navigation
        public Agent Agent { get; set; } = null!;

        public BusinessAccount BusinessAccount { get; set; } = null!;

        public User AssignedByAdmin { get; set; } = null!;
    }
}