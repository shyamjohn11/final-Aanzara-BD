using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities
{
    public class Cart : AuditableEntity
    {
        public Guid CartId { get; set; }

        public Guid UserId { get; set; }

        public Guid? AppliedCouponId { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public Coupon? AppliedCoupon { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}