using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class BrandImage : AuditableEntity
    {
        public Guid BrandImageId { get; set; }

        public Guid BrandId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        // Navigation
        public Brand Brand { get; set; } = null!;
    }
}