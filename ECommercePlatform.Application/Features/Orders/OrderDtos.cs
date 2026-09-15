using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Orders;

public sealed record OrderItemResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

public sealed record OrderAddressResponse
{
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
}

public sealed record OrderSummaryResponse
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GrandTotal { get; init; }
    public int ItemCount { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record OrderDetailResponse
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal ItemsTotal { get; init; }
    public decimal Discount { get; init; }
    public decimal GstAmount { get; init; }
    public decimal DeliveryCharge { get; init; }
    public decimal HandlingFee { get; init; }
    public decimal GrandTotal { get; init; }
    public string? AppliedCouponCode { get; init; }
    public OrderAddressResponse? ShippingAddress { get; init; }
    public IReadOnlyCollection<OrderItemResponse> Items { get; init; } = [];
    public PaymentInfoResponse? Payment { get; init; }
    public IReadOnlyCollection<OrderStatusEventResponse> StatusHistory { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record PaymentInfoResponse
{
    public Guid PaymentId { get; init; }
    public string Method { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? TransactionReference { get; init; }
    public DateTime? PaidAt { get; init; }
}

public sealed record OrderStatusEventResponse
{
    public string Status { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public DateTime ChangedAt { get; init; }
}

public sealed record PlaceOrderResponse
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GrandTotal { get; init; }
    public Guid PaymentId { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
}
