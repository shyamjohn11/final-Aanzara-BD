using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class AgentCommission : AuditableEntity
    {
        public Guid CommissionId { get; set; }

        public Guid AgentId { get; set; }

        public CommissionType CommissionType { get; set; }

        public decimal Value { get; set; }

        public CommissionStatus Status { get; set; }

        // Navigation
        public Agent Agent { get; set; } = null!;
    }
}