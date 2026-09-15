using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class OrderItem : AuditableEntity
    {
        public Guid OrderItemId { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}