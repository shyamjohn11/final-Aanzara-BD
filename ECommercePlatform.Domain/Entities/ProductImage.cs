using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class ProductImage : AuditableEntity
    {
        public Guid ImageId { get; set; }

        public Guid ProductId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public int DisplayOrder { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
    }
}