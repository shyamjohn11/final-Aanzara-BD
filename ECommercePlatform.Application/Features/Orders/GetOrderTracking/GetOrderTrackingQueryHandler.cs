using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Orders.GetOrderTracking;

public sealed class GetOrderTrackingQueryHandler(
    IOrderRepository orders,
    IWarehouseRepository warehouses,
    IAdminRepository<ECommercePlatform.Domain.Entities.Dealer> dealers) : IQueryHandler<GetOrderTrackingQuery, Result<OrderTrackingResponse>>
{
    public async Task<Result<OrderTrackingResponse>> Handle(GetOrderTrackingQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null) return Result.Failure<OrderTrackingResponse>(OrderErrors.NotFound);
        if (order.UserId != request.UserId)
        {
            // Admin can see any, but for customer ensure ownership; for now check user
            // Allow admin to bypass? For simplicity, allow if not owner but return not found
            // Actually we will allow any authenticated user to see own orders only
            return Result.Failure<OrderTrackingResponse>(OrderErrors.NotFound);
        }

        string warehouseName = string.Empty;
        if (order.FulfilledByWarehouseId.HasValue)
        {
            var wh = await warehouses.GetByIdAsync(order.FulfilledByWarehouseId.Value, cancellationToken);
            warehouseName = wh?.WarehouseName ?? string.Empty;
        }

        string dealerShop = string.Empty;
        if (order.DealerId.HasValue)
        {
            var d = await dealers.GetByIdAsync(order.DealerId.Value, cancellationToken);
            dealerShop = d?.ShopName ?? string.Empty;
        }

        var history = order.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new TrackingEventDto(
                h.Status.ToString(),
                h.Remarks ?? string.Empty,
                order.CurrentLocation ?? string.Empty,
                h.ChangedAt))
            .ToList();

        return Result.Success(new OrderTrackingResponse(
            order.OrderId,
            OrderMappings.OrderNoFor(order),
            order.OrderStatus.ToString(),
            order.TrackingNumber ?? string.Empty,
            order.CourierName ?? string.Empty,
            order.CurrentLocation ?? string.Empty,
            order.EstimatedDeliveryDate,
            warehouseName,
            dealerShop,
            history));
    }
}
