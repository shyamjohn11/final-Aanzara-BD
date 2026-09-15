using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class TaxRule : AuditableEntity
    {
        public Guid TaxRuleId { get; set; }

        public Guid? CategoryId { get; set; }

        public decimal GstPercentage { get; set; }

        // Navigation
        public Category? Category { get; set; }
    }
}