using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class StoreImage : AuditableEntity
    {
        public Guid ImageId { get; set; }

        public Guid StoreId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public int DisplayOrder { get; set; }

        public Store Store { get; set; } = null!;
    }
}