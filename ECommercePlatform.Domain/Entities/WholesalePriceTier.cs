using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class WholesalePriceTier : AuditableEntity
    {
        public Guid TierId { get; set; }

        public Guid ProductId { get; set; }

        public int MinQuantity { get; set; }

        public int? MaxQuantity { get; set; }

        public decimal PricePerUnit { get; set; }

        public WholesalePriceTierStatus Status { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
    }
}