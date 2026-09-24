using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Services;

public static class WarehouseLocator
{
    public static Warehouse? FindNearest(
        IReadOnlyList<Warehouse> candidates,
        IReadOnlyList<Inventory> inventories,
        Guid productId,
        int requiredQty,
        string customerCity,
        string customerState,
        double? customerLat,
        double? customerLon)
    {
        var availableWarehouses = inventories
            .Where(i => i.ProductId == productId && (i.StockQuantity - i.ReservedQuantity) >= requiredQty)
            .Select(i => i.WarehouseId)
            .Distinct()
            .ToHashSet();

        var withStock = candidates.Where(w => availableWarehouses.Contains(w.WarehouseId)).ToList();
        if (withStock.Count == 0) return null;
        if (withStock.Count == 1) return withStock[0];

        // Score by proximity: 0 = same city, 1 = same state, 2 = other. If lat/lon present, use haversine.
        Warehouse? best = null;
        double bestScore = double.MaxValue;

        foreach (var wh in withStock)
        {
            double score;
            if (customerLat.HasValue && customerLon.HasValue && wh.Latitude.HasValue && wh.Longitude.HasValue)
            {
                score = Haversine(customerLat.Value, customerLon.Value, wh.Latitude.Value, wh.Longitude.Value);
            }
            else
            {
                var cityMatch = !string.IsNullOrWhiteSpace(customerCity) && string.Equals(wh.City?.Trim(), customerCity.Trim(), StringComparison.OrdinalIgnoreCase);
                var stateMatch = !string.IsNullOrWhiteSpace(customerState) && string.Equals(wh.State?.Trim(), customerState.Trim(), StringComparison.OrdinalIgnoreCase);
                score = cityMatch ? 0 : stateMatch ? 1 : 2;
                // Add small random tie-breaker to distribute load
                score += Random.Shared.NextDouble() * 0.01;
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = wh;
            }
        }

        return best;
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // km
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
