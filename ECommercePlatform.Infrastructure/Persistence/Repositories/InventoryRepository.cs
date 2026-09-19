using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _db;

    public InventoryRepository(ApplicationDbContext db) => _db = db;

    public Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId, Guid warehouseId, CancellationToken cancellationToken)
        => _db.Inventory
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, cancellationToken);

    public async Task<IReadOnlyCollection<Inventory>> GetByProductAsync(
        Guid productId, Guid? warehouseId, CancellationToken cancellationToken)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId);

        if (warehouseId is not null)
        {
            query = query.Where(i => i.WarehouseId == warehouseId);
        }

        return await query.ToListAsync(cancellationToken);
    }

        public async Task<IReadOnlyCollection<Inventory>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        return await _db.Inventory
            .AsNoTracking()
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Inventory>> SearchInventoryAsync(
        Guid? warehouseId, Guid? productId, string? search, string? status, string? category, int page, int pageSize, CancellationToken cancellationToken)
    {
        // Show every product, even those with no Inventory row yet (stock 0), so the admin can add stock.
        // We paginate Products, then left-join Inventory for display.
        var productQuery = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            productQuery = productQuery.Where(p =>
                EF.Functions.Like(p.ProductName, term) ||
                EF.Functions.Like(p.Sku, term));
        }

        if (productId is not null)
        {
            productQuery = productQuery.Where(p => p.ProductId == productId);
        }

        if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
        {
            var cat = category.Trim();
            productQuery = productQuery.Where(p => p.Category != null && p.Category.CategoryName == cat);
        }

        // Status filter needs inventory data; apply after left-join, so handle separately.
        // First get product ids that match status, then filter productQuery.
        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            var s = status.Trim().ToLowerInvariant();
            // For status, we need to consider Inventory rows; products without inventory are "out of stock"
            var invQuery = _db.Inventory.AsNoTracking().AsQueryable();
            if (warehouseId is not null) invQuery = invQuery.Where(i => i.WarehouseId == warehouseId);

            List<Guid> matchingIds;
            if (s == "in stock")
            {
                matchingIds = await invQuery.Where(i => (i.StockQuantity - i.ReservedQuantity) > i.ReorderLevel).Select(i => i.ProductId).Distinct().ToListAsync(cancellationToken);
                productQuery = productQuery.Where(p => matchingIds.Contains(p.ProductId));
            }
            else if (s == "low stock")
            {
                matchingIds = await invQuery.Where(i => (i.StockQuantity - i.ReservedQuantity) > 0 && (i.StockQuantity - i.ReservedQuantity) <= i.ReorderLevel).Select(i => i.ProductId).Distinct().ToListAsync(cancellationToken);
                productQuery = productQuery.Where(p => matchingIds.Contains(p.ProductId));
            }
            else if (s == "out of stock")
            {
                var inStockIds = await invQuery.Where(i => (i.StockQuantity - i.ReservedQuantity) > 0).Select(i => i.ProductId).Distinct().ToListAsync(cancellationToken);
                productQuery = productQuery.Where(p => !inStockIds.Contains(p.ProductId));
            }
        }

        var total = await productQuery.CountAsync(cancellationToken);

        var pagedProducts = await productQuery
            .OrderBy(p => p.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pagedProducts.Count == 0)
            return new PagedResult<Inventory>([], page, pageSize, total);

        var productIds = pagedProducts.Select(p => p.ProductId).ToList();
        var inventories = await _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.Warehouse)
            .Where(i => productIds.Contains(i.ProductId) && (warehouseId == null || i.WarehouseId == warehouseId))
            .ToListAsync(cancellationToken);

        // Group by product for the case of multiple warehouses and no warehouse filter — sum stock.
        var items = new List<Inventory>();
        var defaultWarehouse = warehouseId ?? (await _db.Warehouses.AsNoTracking().Select(w => w.WarehouseId).FirstOrDefaultAsync(cancellationToken));
        if (defaultWarehouse == Guid.Empty) defaultWarehouse = Guid.NewGuid();

        foreach (var product in pagedProducts)
        {
            var invs = inventories.Where(i => i.ProductId == product.ProductId).ToList();
            if (invs.Count == 0)
            {
                // No inventory row yet — synthesize a 0-stock entry so it appears and can be stocked.
                items.Add(new Inventory
                {
                    InventoryId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    WarehouseId = defaultWarehouse,
                    Product = product,
                    Warehouse = null!,
                    StockQuantity = 0,
                    ReservedQuantity = 0,
                    ReorderLevel = 10,
                    DispatchEstimateDays = 0
                });
            }
            else if (warehouseId != null || invs.Count == 1)
            {
                items.AddRange(invs);
            }
            else
            {
                // Multiple warehouses, no filter — aggregate into one row per product for the list.
                var totalStock = invs.Sum(i => i.StockQuantity);
                var totalReserved = invs.Sum(i => i.ReservedQuantity);
                var minReorder = invs.Min(i => i.ReorderLevel);
                items.Add(new Inventory
                {
                    InventoryId = invs[0].InventoryId,
                    ProductId = product.ProductId,
                    WarehouseId = invs[0].WarehouseId,
                    Product = product,
                    Warehouse = invs[0].Warehouse,
                    StockQuantity = totalStock,
                    ReservedQuantity = totalReserved,
                    ReorderLevel = minReorder,
                    DispatchEstimateDays = invs[0].DispatchEstimateDays
                });
            }
        }

        return new PagedResult<Inventory>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Inventory>> SearchLowStockAsync(
        Guid? warehouseId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.StockQuantity <= i.ReorderLevel);

        if (warehouseId is not null)
        {
            query = query.Where(i => i.WarehouseId == warehouseId);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.StockQuantity)
            .ThenBy(i => i.Product.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Inventory>(items, page, pageSize, total);
    }

    public async Task<PagedResult<InventoryMovement>> SearchMovementsAsync(
        Guid productId, Guid? warehouseId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.InventoryMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Where(m => m.ProductId == productId);

        if (warehouseId is not null)
        {
            query = query.Where(m => m.WarehouseId == warehouseId);
        }

        if (from is not null)
        {
            query = query.Where(m => m.CreatedAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(m => m.CreatedAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryMovement>(items, page, pageSize, total);
    }

    public void Add(Inventory inventory) => _db.Inventory.Add(inventory);

    public void AddMovement(InventoryMovement movement) => _db.InventoryMovements.Add(movement);
}
