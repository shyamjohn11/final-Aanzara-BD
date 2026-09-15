using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class CartItem : AuditableEntity
    {
        public Guid CartItemId { get; set; }

        public Guid CartId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public Cart Cart { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}