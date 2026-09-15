using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class DeliveryPartner : AuditableEntity
    {
        public Guid DeliveryPartnerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? VehicleType { get; set; }

        public Guid ZoneId { get; set; }

        public DeliveryPartnerStatus Status { get; set; }

        // Navigation
        public DeliveryZone Zone { get; set; } = null!;
    }
}