using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class OrderStatusHistory : AuditableEntity
    {
        public Guid HistoryId { get; set; }

        public Guid OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public Guid? ChangedByUserId { get; set; }

        public string? Remarks { get; set; }

        public DateTime ChangedAt { get; set; }

        public Order Order { get; set; } = null!;

        public User? ChangedByUser { get; set; }
    }
}