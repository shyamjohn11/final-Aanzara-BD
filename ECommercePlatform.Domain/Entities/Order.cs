using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Domain.Entities
{
    public class Order : AuditableEntity
    {
        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public decimal ItemsTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal GstAmount { get; set; }

        public decimal DeliveryCharge { get; set; }

        public decimal HandlingFee { get; set; }

        public decimal GrandTotal { get; set; }

        public Guid? AppliedCouponId { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public Coupon? AppliedCoupon { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        public ICollection<OrderStatusHistory> StatusHistory { get; set; }
            = new List<OrderStatusHistory>();

        public OrderAddress? ShippingAddress { get; set; }
    }
}