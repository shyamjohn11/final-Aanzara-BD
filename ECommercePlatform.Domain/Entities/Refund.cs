using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Refund : AuditableEntity
    {
        public Guid RefundId { get; set; }

        public Guid OrderId { get; set; }

        public Guid PaymentId { get; set; }

        public decimal Amount { get; set; }

        public string? Reason { get; set; }

        public RefundStatus Status { get; set; }

        public string? GatewayRefundId { get; set; }

        public Order Order { get; set; } = null!;

        public Payment Payment { get; set; } = null!;
    }
}