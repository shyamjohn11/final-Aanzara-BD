using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class InventoryMovement : AuditableEntity
    {
        public Guid MovementId { get; set; }

        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        public InventoryMovementType Type { get; set; }

        public int Quantity { get; set; }

        public string? Reason { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? CreatedByUserId { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;

        public Warehouse Warehouse { get; set; } = null!;

        public Order? Order { get; set; }

        public User? CreatedByUser { get; set; }
    }
}