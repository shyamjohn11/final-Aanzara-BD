using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class SavedItem : AuditableEntity
    {
        public Guid SavedItemId { get; set; }

        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}