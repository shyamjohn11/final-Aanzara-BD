using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class ShopLedger : AuditableEntity
    {
        public Guid LedgerId { get; set; }

        public Guid BusinessAccountId { get; set; }

        public Guid? OrderId { get; set; }

        public LedgerType Type { get; set; }

        public decimal Amount { get; set; }

        public string? Note { get; set; }

        // Navigation
        public BusinessAccount BusinessAccount { get; set; } = null!;

        public Order? Order { get; set; }
    }
}