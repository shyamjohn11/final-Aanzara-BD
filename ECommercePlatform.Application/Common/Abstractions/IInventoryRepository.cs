using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Inventory>> GetByProductAsync(Guid productId, Guid? warehouseId, CancellationToken cancellationToken);

        /// <summary>
    /// Every inventory row for the given products, across all warehouses. Used to
    /// compute per-product availability for a set of products in one query — the
    /// cart summary being the first caller — rather than one call per product.
    /// </summary>
    Task<IReadOnlyCollection<Inventory>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    Task<PagedResult<Inventory>> SearchInventoryAsync(
        Guid? warehouseId,
        Guid? productId,
        string? search,
        string? status,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PagedResult<Inventory>> SearchLowStockAsync(
        Guid? warehouseId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PagedResult<InventoryMovement>> SearchMovementsAsync(
        Guid productId,
        Guid? warehouseId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(Inventory inventory);

    void AddMovement(InventoryMovement movement);
}
