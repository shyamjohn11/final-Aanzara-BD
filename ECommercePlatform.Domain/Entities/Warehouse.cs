using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Warehouse : AuditableEntity
    {
        public Guid WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public WarehouseStatus Status { get; set; }
    }
}