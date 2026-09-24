using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetNearestWarehouse;

public sealed class GetNearestWarehouseQueryHandler(
    IWarehouseRepository warehouses,
    IInventoryRepository inventory) : IQueryHandler<GetNearestWarehouseQuery, Result<GetNearestWarehouseResponse>>
{
    public async Task<Result<GetNearestWarehouseResponse>> Handle(GetNearestWarehouseQuery request, CancellationToken cancellationToken)
    {
        var page = await warehouses.SearchAsync(null, null, 1, 100, cancellationToken);
        var all = page.Items.ToList();

        var inventories = await inventory.GetByProductIdsAsync([request.ProductId], cancellationToken);

        var nearest = WarehouseLocator.FindNearest(
            all,
            inventories.ToList(),
            request.ProductId,
            1,
            request.City ?? string.Empty,
            request.State ?? string.Empty,
            request.Latitude,
            request.Longitude);

        if (nearest is null)
        {
            return Result.Failure<GetNearestWarehouseResponse>(Error.NotFound("warehouse.not_found", "No warehouse with stock found for this product near your location."));
        }

        var inv = inventories.FirstOrDefault(i => i.WarehouseId == nearest.WarehouseId && i.ProductId == request.ProductId);
        var available = inv is null ? 0 : inv.StockQuantity - inv.ReservedQuantity;

        // Compute distance for response
        double distance = 0;
        if (request.Latitude.HasValue && request.Longitude.HasValue && nearest.Latitude.HasValue && nearest.Longitude.HasValue)
        {
            // Haversine already computed inside locator, recompute for display
            distance = Haversine(request.Latitude.Value, request.Longitude.Value, nearest.Latitude.Value, nearest.Longitude.Value);
        }

        return Result.Success(new GetNearestWarehouseResponse(
            nearest.WarehouseId,
            nearest.WarehouseName,
            nearest.City ?? string.Empty,
            nearest.State ?? string.Empty,
            nearest.Latitude,
            nearest.Longitude,
            available,
            Math.Round(distance, 1)));
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
