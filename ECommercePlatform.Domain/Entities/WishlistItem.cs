using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class WishlistItem : AuditableEntity
    {
        public Guid WishlistItemId { get; set; }

        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}