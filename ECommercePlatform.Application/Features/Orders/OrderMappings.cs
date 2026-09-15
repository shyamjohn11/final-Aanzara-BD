using ECommercePlatform.Domain.Entities;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders;

/// <summary>
/// Order → response shaping shared by the customer endpoints. Kept in one place
/// so the list and detail views can never disagree about what an order looks like.
/// </summary>
internal static class OrderMappings
{
    // The schema has no order-number column; the customer-facing number is
    // derived from the id so it is stable but not guessably sequential.
    public static string OrderNoFor(Order order)
        => $"ORD-{order.OrderId.ToString("N")[..8].ToUpperInvariant()}";

    public static OrderSummaryResponse ToSummary(Order order)
    {
        var payment = LatestPayment(order);

        return new OrderSummaryResponse
        {
            OrderId = order.OrderId,
            OrderNo = OrderNoFor(order),
            Status = order.OrderStatus.ToString(),
            GrandTotal = order.GrandTotal,
            ItemCount = order.OrderItems.Sum(i => i.Quantity),
            PaymentStatus = payment?.Status.ToString(),
            PaymentMethod = payment?.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt
        };
    }

    public static OrderDetailResponse ToDetail(Order order)
    {
        var payment = LatestPayment(order);

        return new OrderDetailResponse
        {
            OrderId = order.OrderId,
            OrderNo = OrderNoFor(order),
            Status = order.OrderStatus.ToString(),
            ItemsTotal = order.ItemsTotal,
            Discount = order.Discount,
            GstAmount = order.GstAmount,
            DeliveryCharge = order.DeliveryCharge,
            HandlingFee = order.HandlingFee,
            GrandTotal = order.GrandTotal,
            AppliedCouponCode = order.AppliedCoupon?.CouponCode,
            ShippingAddress = order.ShippingAddress is null ? null : new OrderAddressResponse
            {
                RecipientName = order.ShippingAddress.RecipientName,
                RecipientPhone = order.ShippingAddress.RecipientPhone,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                Pincode = order.ShippingAddress.Pincode
            },
            Items = order.OrderItems
                .Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
                .ToList(),
            Payment = payment is null ? null : new PaymentInfoResponse
            {
                PaymentId = payment.PaymentId,
                Method = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                TransactionReference = payment.GatewayTransactionId,
                PaidAt = payment.PaidAt
            },
            StatusHistory = order.StatusHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new OrderStatusEventResponse
                {
                    Status = h.Status.ToString(),
                    Remarks = h.Remarks,
                    ChangedAt = h.ChangedAt
                })
                .ToList(),
            CreatedAt = order.CreatedAt
        };
    }

    private static Payment? LatestPayment(Order order) =>
        order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
}
