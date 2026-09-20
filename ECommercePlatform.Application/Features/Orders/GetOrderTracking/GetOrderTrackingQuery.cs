using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders.GetOrderTracking;

public sealed record GetOrderTrackingQuery(Guid UserId, Guid OrderId) : IQuery<Result<OrderTrackingResponse>>;

public sealed record ShipmentTrackingInfo
{
    public string CourierName { get; init; } = "Aanzara Logistics";
    public string? TrackingNumber { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTime? ShippedAt { get; init; }
    public DateTime? EstimatedDelivery { get; init; }
    public DateTime? DeliveredAt { get; init; }
}

public sealed record OrderTrackingResponse
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal GrandTotal { get; init; }
    public ShipmentTrackingInfo Shipment { get; init; } = new();
    public IReadOnlyCollection<OrderStatusEventResponse> Timeline { get; init; } = [];
    public int ProgressPercent { get; init; }
    public string EstimatedDeliveryText { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
}
