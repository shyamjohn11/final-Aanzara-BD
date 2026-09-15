using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class DeliveryZone : AuditableEntity
    {
        public Guid ZoneId { get; set; }

        public string ZoneName { get; set; } = string.Empty;

        public string? Pincodes { get; set; }

        public int RadiusKm { get; set; }
    }
}