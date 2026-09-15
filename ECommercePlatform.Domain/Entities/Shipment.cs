using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Shipment : AuditableEntity
    {
        public Guid ShipmentId { get; set; }

        public Guid OrderId { get; set; }

        public string? CourierName { get; set; }

        public string? TrackingNumber { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public ShipmentStatus Status { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
    }
}