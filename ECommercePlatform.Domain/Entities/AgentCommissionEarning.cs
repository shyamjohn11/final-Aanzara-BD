using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class AgentCommissionEarning : AuditableEntity
    {
        public Guid EarningId { get; set; }

        public Guid AgentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid CommissionId { get; set; }

        public decimal AmountEarned { get; set; }

        public EarningStatus Status { get; set; }

        // Navigation
        public Agent Agent { get; set; } = null!;

        public Order Order { get; set; } = null!;

        public AgentCommission Commission { get; set; } = null!;
    }
}