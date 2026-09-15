using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Store : AuditableEntity
    {
        public Guid StoreId { get; set; }

        public string StoreName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string? ContactNumber { get; set; }

        public string? OpeningHours { get; set; }

        public StoreStatus Status { get; set; }

        public ICollection<StoreImage> Images { get; set; }
            = new List<StoreImage>();
    }
}