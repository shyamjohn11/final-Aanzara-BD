using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class SubCategoryImage : AuditableEntity
    {
        public Guid ImageId { get; set; }

        public Guid SubCategoryId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public int DisplayOrder { get; set; }

        // Navigation
        public SubCategory SubCategory { get; set; } = null!;
    }
}
