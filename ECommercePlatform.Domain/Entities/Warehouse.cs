using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Warehouse : AuditableEntity
    {
        public Guid WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public WarehouseStatus Status { get; set; }
    }
}