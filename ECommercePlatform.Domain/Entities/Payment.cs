using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public Guid PaymentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string? PaymentGateway { get; set; }

        public string? GatewayTransactionId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        public PaymentStatus Status { get; set; }

        public string? FailureReason { get; set; }

        public DateTime? PaidAt { get; set; }

        public Order Order { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}