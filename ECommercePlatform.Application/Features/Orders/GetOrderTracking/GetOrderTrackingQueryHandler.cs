using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Orders;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders.GetOrderTracking;

public sealed class GetOrderTrackingQueryHandler(
    IOrderRepository orders) : IQueryHandler<GetOrderTrackingQuery, Result<OrderTrackingResponse>>
{
    // Order flow for progress: Pending -> Confirmed -> Shipped -> Delivered
    private static readonly string[] Flow = ["Pending", "Confirmed", "Shipped", "OutForDelivery", "Delivered"];

    public async Task<Result<OrderTrackingResponse>> Handle(
        GetOrderTrackingQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.UserId != request.UserId)
            return Result.Failure<OrderTrackingResponse>(OrderErrors.NotFound);

        var status = order.OrderStatus.ToString();
        var history = order.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusEventResponse
            {
                Status = h.Status.ToString(),
                Remarks = h.Remarks,
                ChangedAt = h.ChangedAt
            }).ToArray();

        // Shipments are stored separately; for live tracking we synthesize from order status
        // (no extra DB round-trip needed for the demo flow). Real courier integration would
        // hydrate from Shipments/DeliveryAssignments here.
        var shipInfo = new ShipmentTrackingInfo
        {
            CourierName = "Aanzara Logistics",
            TrackingNumber = $"TRK-{order.OrderId.ToString("N")[..8].ToUpperInvariant()}",
            Status = MapShipmentStatus(status),
            ShippedAt = status is "Shipped" or "OutForDelivery" or "Delivered" ? order.UpdatedAt.UtcDateTime : null,
            EstimatedDelivery = order.CreatedAt.AddDays(4).UtcDateTime,
            DeliveredAt = status == "Delivered" ? order.UpdatedAt.UtcDateTime : null
        };

        var idx = Array.FindIndex(Flow, s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        if (idx < 0 && status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) idx = -1;
        var progress = status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ? 0 :
                       status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ? 100 :
                       idx < 0 ? 15 : (int)Math.Round((idx + 1) / (double)Flow.Length * 100);

        var etaText = shipInfo.EstimatedDelivery.HasValue
            ? shipInfo.EstimatedDelivery.Value.ToString("MMM dd") + $" · {shipInfo.CourierName}"
            : "—";

        return Result.Success(new OrderTrackingResponse
        {
            OrderId = order.OrderId,
            OrderNo = OrderMappings.OrderNoFor(order),
            Status = status,
            GrandTotal = order.GrandTotal,
            Shipment = shipInfo,
            Timeline = history,
            ProgressPercent = progress,
            EstimatedDeliveryText = etaText,
            UpdatedAt = order.UpdatedAt
        });
    }

    private static string MapShipmentStatus(string orderStatus) => orderStatus switch
    {
        "Pending" => "Pending",
        "Confirmed" => "Processing",
        "Shipped" => "InTransit",
        "Delivered" => "Delivered",
        "Cancelled" => "Cancelled",
        _ => orderStatus
    };
}
