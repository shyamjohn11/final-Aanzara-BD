using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Agent : AuditableEntity
    {
        public Guid AgentId { get; set; }

        public Guid UserId { get; set; }

        public Guid CreatedByAdminId { get; set; }

        // Currently not FK
        public Guid? AssignedStoreId { get; set; }

        public string? EmployeeCode { get; set; }

        public AgentStatus Status { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public User CreatedByAdmin { get; set; } = null!;
    }
}