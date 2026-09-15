using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Coupon : AuditableEntity
    {
        public Guid CouponId { get; set; }

        public string CouponCode { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal MinOrderValue { get; set; }

        public int UsageLimitPerUser { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public CouponStatus Status { get; set; }
    }
}