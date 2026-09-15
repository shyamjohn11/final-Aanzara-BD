using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Inventory : AuditableEntity
    {
        public Guid InventoryId { get; set; }

        public Guid ProductId { get; set; }

        public Guid WarehouseId { get; set; }

        public int StockQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public int DispatchEstimateDays { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;

        public Warehouse Warehouse { get; set; } = null!;
    }
}