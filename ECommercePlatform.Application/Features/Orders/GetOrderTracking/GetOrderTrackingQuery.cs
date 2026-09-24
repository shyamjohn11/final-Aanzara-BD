using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders.GetOrderTracking;

public sealed record GetOrderTrackingQuery(Guid UserId, Guid OrderId) : IQuery<Result<OrderTrackingResponse>>;

public sealed record TrackingEventDto(
    string Status,
    string Remarks,
    string Location,
    DateTime ChangedAt
);

public sealed record OrderTrackingResponse(
    Guid OrderId,
    string OrderNo,
    string Status,
    string TrackingNumber,
    string CourierName,
    string CurrentLocation,
    DateTimeOffset? EstimatedDeliveryDate,
    string WarehouseName,
    string DealerShopName,
    IReadOnlyList<TrackingEventDto> History);
