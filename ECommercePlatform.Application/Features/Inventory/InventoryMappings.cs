using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Inventory;

public static class InventoryMappings
{
    public static InventoryResponse ToResponse(this Domain.Entities.Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        ProductId = i.ProductId,
        ProductName = i.Product?.ProductName ?? string.Empty,
        Sku = i.Product?.Sku ?? string.Empty,
        Category = i.Product?.Category?.CategoryName ?? "Uncategorized",
        Brand = string.Empty,
        WarehouseId = i.WarehouseId,
        WarehouseName = i.Warehouse?.WarehouseName ?? string.Empty,
        StockQuantity = i.StockQuantity,
        ReservedQuantity = i.ReservedQuantity,
        ReorderLevel = i.ReorderLevel,
        DispatchEstimateDays = i.DispatchEstimateDays,
        UpdatedAt = i.UpdatedAt
    };

    /// <summary>
    /// Fills ImageUrl with the primary product image streaming route, using one
    /// batch lookup for the whole page (same pattern as ProductSummaryEnrichment).
    /// </summary>
    public static async Task<InventoryResponse[]> ToEnrichedResponsesAsync(
        this IReadOnlyCollection<Domain.Entities.Inventory> items,
        IProductImageRepository productImages,
        CancellationToken cancellationToken)
    {
        var responses = items.Select(i => i.ToResponse()).ToArray();

        if (responses.Length == 0)
        {
            return responses;
        }

        var productIds = responses.Select(r => r.ProductId).Distinct().ToArray();
        var primaryImage = (await productImages.GetByProductIdsAsync(productIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .First());

        for (var i = 0; i < responses.Length; i++)
        {
            if (primaryImage.TryGetValue(responses[i].ProductId, out var image))
            {
                responses[i] = responses[i] with
                {
                    ImageUrl = $"/api/v1/products/{responses[i].ProductId}/images/{image.ImageId}/file"
                };
            }
        }

        return responses;
    }

    public static InventoryWarehouseStockDto ToWarehouseStockDto(this Domain.Entities.Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        WarehouseId = i.WarehouseId,
        WarehouseName = i.Warehouse?.WarehouseName ?? string.Empty,
        StockQuantity = i.StockQuantity,
        ReservedQuantity = i.ReservedQuantity,
        ReorderLevel = i.ReorderLevel,
        DispatchEstimateDays = i.DispatchEstimateDays,
        UpdatedAt = i.UpdatedAt
    };

    public static InventoryMovementResponse ToResponse(this InventoryMovement m) => new()
    {
        MovementId = m.MovementId,
        ProductId = m.ProductId,
        ProductName = m.Product?.ProductName ?? string.Empty,
        Sku = m.Product?.Sku ?? string.Empty,
        WarehouseId = m.WarehouseId,
        WarehouseName = m.Warehouse?.WarehouseName ?? string.Empty,
        Type = m.Type,
        Quantity = m.Quantity,
        Reason = m.Reason,
        OrderId = m.OrderId,
        CreatedByUserId = m.CreatedByUserId,
        CreatedAt = m.CreatedAt
    };
}
