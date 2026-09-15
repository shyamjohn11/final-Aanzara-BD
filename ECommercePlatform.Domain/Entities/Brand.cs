using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Brand : AuditableEntity
    {
        public Guid BrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsOnSale { get; set; }

        public BrandStatus Status { get; set; }

        public ICollection<BrandImage> Images { get; set; }
            = new List<BrandImage>();

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}

public enum BrandStatus
{
    Active,
    Inactive
}