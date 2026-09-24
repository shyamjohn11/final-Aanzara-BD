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

        // Warehouse-wise fulfillment: nearest warehouse selected based on customer location
        public Guid? FulfilledByWarehouseId { get; set; }

        public Warehouse? FulfilledByWarehouse { get; set; }

        // Dealer shop fulfillment: when order contains dealer products
        public Guid? DealerId { get; set; }

        public Dealer? Dealer { get; set; }

        // Live tracking
        public string? TrackingNumber { get; set; }

        public string? CourierName { get; set; }

        public DateTimeOffset? EstimatedDeliveryDate { get; set; }

        public string? CurrentLocation { get; set; }

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