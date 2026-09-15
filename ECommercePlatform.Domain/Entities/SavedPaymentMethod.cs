using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class SavedPaymentMethod : AuditableEntity
    {
        public Guid SavedMethodId { get; set; }

        public Guid UserId { get; set; }

        public SavedPaymentMethodType PaymentMethod { get; set; }

        public string GatewayToken { get; set; } = string.Empty;

        public string DisplayLabel { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public User User { get; set; } = null!;
    }
}